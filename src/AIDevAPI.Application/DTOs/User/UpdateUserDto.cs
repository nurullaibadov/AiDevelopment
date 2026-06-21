using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.User;

public class UpdateUserDto
{
    [MaxLength(50)]
    public string? FirstName { get; set; }

    [MaxLength(50)]
    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }
}
