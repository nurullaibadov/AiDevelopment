using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.User;

namespace AIDevAPI.Application.Interfaces.Services;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(PaginationParams pagination);
    Task<UserDto> GetUserByIdAsync(Guid id);
    Task<UserDto> GetCurrentUserAsync(Guid id);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task DeleteUserAsync(Guid id);
    Task LockUserAsync(Guid id);
    Task UnlockUserAsync(Guid id);
    Task<UserDto> AssignRoleAsync(Guid id, string roleName);
    Task<UserDto> RemoveRoleAsync(Guid id, string roleName);
}
