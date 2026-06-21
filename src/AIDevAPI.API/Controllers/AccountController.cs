using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.User;
using AIDevAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public AccountController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>Daxil olmuş istifadəçinin profilini gətirmək</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        var userId = _currentUserService.UserId!.Value;
        var user = await _userService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user));
    }

    /// <summary>Daxil olmuş istifadəçinin profilini yeniləmək</summary>
    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateMe([FromBody] UpdateUserDto dto)
    {
        var userId = _currentUserService.UserId!.Value;
        var user = await _userService.UpdateUserAsync(userId, dto);
        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Profil uğurla yeniləndi."));
    }
}
