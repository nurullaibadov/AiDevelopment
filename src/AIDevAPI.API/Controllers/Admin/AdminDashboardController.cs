using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.Dashboard;
using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Domain.Entities;
using AIDevAPI.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public AdminDashboardController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Admin panel üçün ümumi statistika</summary>
    [HttpGet("stats")]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var allUsers = _userManager.Users.ToList();
        var allProjects = _unitOfWork.Projects.Query().ToList();

        var lockedCount = 0;
        foreach (var u in allUsers)
        {
            if (await _userManager.IsLockedOutAsync(u)) lockedCount++;
        }

        var stats = new DashboardStatsDto
        {
            TotalUsers = allUsers.Count,
            ActiveUsers = allUsers.Count(u => u.IsActive),
            LockedUsers = lockedCount,
            TotalRoles = _roleManager.Roles.Count(),
            TotalProjects = allProjects.Count,
            CompletedProjects = allProjects.Count(p => p.Status == ProjectStatus.Completed),
            InProgressProjects = allProjects.Count(p => p.Status == ProjectStatus.InProgress),
            NewUsersLast7Days = allUsers.Count(u => u.CreatedAt >= sevenDaysAgo),
            NewProjectsLast7Days = allProjects.Count(p => p.CreatedAt >= sevenDaysAgo)
        };

        return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats));
    }
}
