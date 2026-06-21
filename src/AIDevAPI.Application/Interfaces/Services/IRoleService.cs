using AIDevAPI.Application.DTOs.Role;

namespace AIDevAPI.Application.Interfaces.Services;

public interface IRoleService
{
    Task<List<RoleDto>> GetRolesAsync();
    Task<RoleDto> GetRoleByIdAsync(Guid id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task DeleteRoleAsync(Guid id);
}
