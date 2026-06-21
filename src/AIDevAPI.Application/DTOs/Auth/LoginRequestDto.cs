using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email və ya istifadəçi adı mütləqdir")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə mütləqdir")]
    public string Password { get; set; } = string.Empty;
}
