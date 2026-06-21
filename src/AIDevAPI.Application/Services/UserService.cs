using AIDevAPI.Application.Common;
using AIDevAPI.Application.Common.Exceptions;
using AIDevAPI.Application.DTOs.User;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AIDevAPI.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(PaginationParams pagination)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(term) ||
                u.UserName!.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var totalCount = query.Count();

        var users = query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            userDtos.Add(await MapToDtoAsync(user));
        }

        return new PagedResult<UserDto>
        {
            Items = userDtos,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid id) => await GetUserByIdAsync(id);

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        return await MapToDtoAsync(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());
    }

    public async Task LockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        user.IsActive = false;
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        await _userManager.UpdateAsync(user);
    }

    public async Task UnlockUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        user.IsActive = true;
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateAsync(user);
    }

    public async Task<UserDto> AssignRoleAsync(Guid id, string roleName)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        if (await _userManager.IsInRoleAsync(user, roleName))
            throw new BadRequestException("İstifadəçi artıq bu rola malikdir.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        return await MapToDtoAsync(user);
    }

    public async Task<UserDto> RemoveRoleAsync(Guid id, string roleName)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
            throw new NotFoundException("İstifadəçi", id);

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            throw new ValidationAppException(result.Errors.Select(e => e.Description).ToList());

        return await MapToDtoAsync(user);
    }

    private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var lockedOut = await _userManager.IsLockedOutAsync(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LockedOut = lockedOut,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList()
        };
    }
}
