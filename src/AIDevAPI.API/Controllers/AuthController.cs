using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.Auth;
using AIDevAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>Yeni istifadəçi qeydiyyatı</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request, _currentUserService.IpAddress);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Qeydiyyat uğurla tamamlandı."));
    }

    /// <summary>İstifadəçi girişi</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request, _currentUserService.IpAddress);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Giriş uğurludur."));
    }

    /// <summary>Access tokeni refresh token ilə yeniləmək</summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request, _currentUserService.IpAddress);
        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Token uğurla yeniləndi."));
    }

    /// <summary>Refresh tokeni ləğv etmək (çıxış / logout)</summary>
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        await _authService.RevokeTokenAsync(request.RefreshToken, _currentUserService.IpAddress);
        return Ok(ApiResponse.SuccessResponse("Çıxış uğurla tamamlandı."));
    }

    /// <summary>Şifrəni unutdum - email vasitəsilə bərpa linki göndərmək</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse.SuccessResponse("Əgər bu email qeydiyyatdan keçibsə, şifrə bərpa linki göndərildi."));
    }

    /// <summary>Email ilə göndərilən token vasitəsilə şifrəni sıfırlamaq</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.SuccessResponse("Şifrəniz uğurla yeniləndi. İndi yeni şifrə ilə daxil ola bilərsiniz."));
    }

    /// <summary>Email təsdiqi</summary>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse>> ConfirmEmail([FromBody] ConfirmEmailRequestDto request)
    {
        await _authService.ConfirmEmailAsync(request);
        return Ok(ApiResponse.SuccessResponse("Email uğurla təsdiqləndi."));
    }

    /// <summary>Daxil olmuş istifadəçinin şifrəni dəyişməsi</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = _currentUserService.UserId!.Value;
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(ApiResponse.SuccessResponse("Şifrəniz uğurla dəyişdirildi."));
    }
}
