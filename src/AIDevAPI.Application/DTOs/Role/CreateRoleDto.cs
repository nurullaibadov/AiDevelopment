using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.Role;

public class CreateRoleDto
{
    [Required(ErrorMessage = "Rol adı mütləqdir")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
