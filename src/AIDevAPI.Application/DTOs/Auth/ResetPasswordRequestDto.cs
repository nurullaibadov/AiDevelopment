using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Auth;

public class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token mütləqdir")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifrə mütləqdir")]
    [MinLength(6, ErrorMessage = "Şifrə ən azı 6 simvol olmalıdır")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword", ErrorMessage = "Şifrələr uyğun gəlmir")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
