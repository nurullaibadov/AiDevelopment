using System.ComponentModel.DataAnnotations;

namespace AIDevAPI.Application.DTOs.User;

public class UpdateUserRoleDto
{
    [Required(ErrorMessage = "Rol adı mütləqdir")]
    public string RoleName { get; set; } = string.Empty;
}
