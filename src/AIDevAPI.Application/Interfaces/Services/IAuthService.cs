using AIDevAPI.Application.DTOs.Auth;

namespace AIDevAPI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string ipAddress);
    Task RevokeTokenAsync(string refreshToken, string ipAddress);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task ConfirmEmailAsync(ConfirmEmailRequestDto request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}
