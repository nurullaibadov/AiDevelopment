using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Auth;

public class RevokeTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
