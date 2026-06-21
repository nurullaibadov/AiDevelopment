using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.Role;
using AIDevAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers.Admin;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Policy = "AdminOnly")]
public class AdminRolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public AdminRolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>Bütün rolların siyahısı</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles()
    {
        var roles = await _roleService.GetRolesAsync();
        return Ok(ApiResponse<List<RoleDto>>.SuccessResponse(roles));
    }

    /// <summary>ID-yə görə rol məlumatı</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRoleById(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(role));
    }

    /// <summary>Yeni rol yaratmaq</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleDto dto)
    {
        var role = await _roleService.CreateRoleAsync(dto);
        return Ok(ApiResponse<RoleDto>.SuccessResponse(role, "Rol uğurla yaradıldı."));
    }

    /// <summary>Rolun silinməsi</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteRole(Guid id)
    {
        await _roleService.DeleteRoleAsync(id);
        return Ok(ApiResponse.SuccessResponse("Rol uğurla silindi."));
    }
}
