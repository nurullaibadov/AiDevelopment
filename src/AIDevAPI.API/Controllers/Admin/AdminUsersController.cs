using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.User;
using AIDevAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Bütün istifadəçilərin siyahısı (səhifələnmiş, axtarışlı)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers([FromQuery] PaginationParams pagination)
    {
        var result = await _userService.GetUsersAsync(pagination);
        return Ok(ApiResponse<PagedResult<UserDto>>.SuccessResponse(result));
    }

    /// <summary>ID-yə görə istifadəçi məlumatı</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>İstifadəçi məlumatlarının admin tərəfindən yenilənməsi</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "İstifadəçi məlumatları yeniləndi."));
    }

    /// <summary>İstifadəçinin silinməsi</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        await _userService.DeleteUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("İstifadəçi uğurla silindi."));
    }

    /// <summary>İstifadəçinin bloklanması</summary>
    [HttpPost("{id:guid}/lock")]
    public async Task<ActionResult<ApiResponse>> LockUser(Guid id)
    {
        await _userService.LockUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("İstifadəçi bloklandı."));
    }

    /// <summary>İstifadəçinin blokdan çıxarılması</summary>
    [HttpPost("{id:guid}/unlock")]
    public async Task<ActionResult<ApiResponse>> UnlockUser(Guid id)
    {
        await _userService.UnlockUserAsync(id);
        return Ok(ApiResponse.SuccessResponse("İstifadəçinin bloku götürüldü."));
    }

    /// <summary>İstifadəçiyə rol təyin etmək</summary>
    [HttpPost("{id:guid}/roles")]
    public async Task<ActionResult<ApiResponse<UserDto>>> AssignRole(Guid id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _userService.AssignRoleAsync(id, dto.RoleName);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Rol uğurla təyin edildi."));
    }

    /// <summary>İstifadəçidən rol götürmək</summary>
    [HttpDelete("{id:guid}/roles/{roleName}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> RemoveRole(Guid id, string roleName)
    {
        var user = await _userService.RemoveRoleAsync(id, roleName);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Rol uğurla götürüldü."));
    }
}
