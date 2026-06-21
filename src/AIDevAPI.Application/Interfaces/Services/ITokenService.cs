using System.Security.Claims;
using AIDevAPI.Domain.Entities;

namespace AIDevAPI.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiry();
    DateTime GetRefreshTokenExpiry();
}
