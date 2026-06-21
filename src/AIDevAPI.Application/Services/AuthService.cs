using AIDevAPI.Application.Common.Exceptions;
using AIDevAPI.Application.DTOs.Auth;
using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Domain.Constants;
using AIDevAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AIDevAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress)
    {
        var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingUserByEmail != null)
            throw new BadRequestException("Bu email artıq qeydiyyatdan keçib.");

        var existingUserByUserName = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserByUserName != null)
            throw new BadRequestException("Bu istifadəçi adı artıq mövcuddur.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        if (!await _roleManager.RoleExistsAsync(RoleConstants.User))
            await _roleManager.CreateAsync(new ApplicationRole(RoleConstants.User, "Standart istifadəçi rolu"));

        await _userManager.AddToRoleAsync(user, RoleConstants.User);

        // Email təsdiq linki hazırlanır
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(confirmationToken);
        var frontendUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "https://localhost:3000";
        var confirmationLink = $"{frontendUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email!, user.FullName);
            await _emailService.SendEmailConfirmationAsync(user.Email!, user.FullName, confirmationLink);
        }
        catch
        {
            // Email göndərilməsi qeydiyyat prosesini bloklamamalıdır
        }

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseAsync(user, roles, ipAddress);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
    {
        var user = request.EmailOrUserName.Contains('@')
            ? await _userManager.FindByEmailAsync(request.EmailOrUserName)
            : await _userManager.FindByNameAsync(request.EmailOrUserName);

        if (user == null)
            throw new UnauthorizedAppException("Email/istifadəçi adı və ya şifrə yanlışdır.");

        if (!user.IsActive)
            throw new ForbiddenException("Hesabınız deaktiv edilib. Zəhmət olmasa administratorla əlaqə saxlayın.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new ForbiddenException("Hesabınız müvəqqəti olaraq bloklanıb. Bir az sonra yenidən cəhd edin.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedAppException("Email/istifadəçi adı və ya şifrə yanlışdır.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuthResponseAsync(user, roles, ipAddress);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedAppException("Token etibarsızdır.");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAppException("Token etibarsızdır.");

        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);
        if (storedToken == null || storedToken.UserId != userId || !storedToken.IsActive)
            throw new UnauthorizedAppException("Refresh token etibarsızdır və ya vaxtı bitib.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            throw new UnauthorizedAppException("İstifadəçi tapılmadı və ya deaktivdir.");

        // Köhnə tokeni ləğv edib, rotation ilə yenisini yaradırıq
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;

        var roles = await _userManager.GetRolesAsync(user);
        var response = await BuildAuthResponseAsync(user, roles, ipAddress);

        storedToken.ReplacedByToken = response.RefreshToken;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();

        return response;
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (storedToken == null || !storedToken.IsActive)
            throw new NotFoundException("Refresh token", refreshToken);

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Təhlükəsizlik səbəbindən istifadəçi tapılmasa belə xəta atmırıq
        // (email enumeration hücumlarının qarşısının alınması üçün)
        if (user == null || !user.IsActive)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(user.Email!);
        var frontendUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "https://localhost:3000";
        var resetLink = $"{frontendUrl}/reset-password?email={encodedEmail}&token={encodedToken}";

        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            throw new NotFoundException("İstifadəçi", request.Email);

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        // Şifrə dəyişdikdə bütün aktiv sessiyaları (refresh tokenləri) ləğv edirik
        var activeTokens = await _unitOfWork.RefreshTokens.GetActiveTokensByUserIdAsync(user.Id);
        foreach (var t in activeTokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(t);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("İstifadəçi", request.UserId);

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", userId);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, IList<string> roles, string ipAddress)
    {
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = _tokenService.GetRefreshTokenExpiry(),
            CreatedByIp = ipAddress
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles.ToList(),
            AccessToken = accessToken,
            AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry(),
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt
        };
    }
}
