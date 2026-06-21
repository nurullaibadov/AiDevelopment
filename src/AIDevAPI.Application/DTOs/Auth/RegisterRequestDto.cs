using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Ad mütləqdir")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad mütləqdir")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email mütləqdir")]
    [EmailAddress(ErrorMessage = "Email formatı düzgün deyil")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "İstifadəçi adı mütləqdir")]
    [MinLength(3, ErrorMessage = "İstifadəçi adı ən azı 3 simvol olmalıdır")]
    [MaxLength(30)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə mütləqdir")]
    [MinLength(6, ErrorMessage = "Şifrə ən azı 6 simvol olmalıdır")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrə təkrarı mütləqdir")]
    [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
