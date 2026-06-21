using AIDevAPI.Application.Common.Exceptions;
using AIDevAPI.Application.DTOs.Role;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AIDevAPI.Application.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = _roleManager.Roles.ToList();
        var dtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            dtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UserCount = usersInRole.Count
            });
        }

        return dtos;
    }

    public async Task<RoleDto> GetRoleByIdAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            throw new NotFoundException("Rol", id);

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            UserCount = usersInRole.Count
        };
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            throw new BadRequestException("Bu adda rol artıq mövcuddur.");

        var role = new ApplicationRole(dto.Name, dto.Description);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        return new RoleDto { Id = role.Id, Name = role.Name!, Description = role.Description, UserCount = 0 };
    }

    public async Task DeleteRoleAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            throw new NotFoundException("Rol", id);

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Count > 0)
            throw new BadRequestException("Bu rola bağlı istifadəçilər olduğu üçün rol silinə bilməz.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());
    }
}
