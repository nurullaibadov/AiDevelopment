using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email mütləqdir")]
    [EmailAddress(ErrorMessage = "Email formatı düzgün deyil")]
    public string Email { get; set; } = string.Empty;
}
