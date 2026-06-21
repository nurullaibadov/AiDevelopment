using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.Project;
using AIDevAPI.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDevAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ICurrentUserService _currentUserService;

    public ProjectsController(IProjectService projectService, ICurrentUserService currentUserService)
    {
        _projectService = projectService;
        _currentUserService = currentUserService;
    }

    private bool IsAdmin => _currentUserService.IsInRole("Admin");

    /// <summary>Layihələrin siyahısı (Admin hamısını, adi istifadəçi yalnız öz layihələrini görür)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjects([FromQuery] PaginationParams pagination)
    {
        var userId = _currentUserService.UserId!.Value;
        var result = await _projectService.GetProjectsAsync(pagination, userId, IsAdmin);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.SuccessResponse(result));
    }

    /// <summary>ID-yə görə layihə</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProjectById(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;
        var project = await _projectService.GetProjectByIdAsync(id, userId, IsAdmin);
        return Ok(ApiResponse<ProjectDto>.SuccessResponse(project));
    }

    /// <summary>Yeni AI layihəsi yaratmaq</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectDto dto)
    {
        var userId = _currentUserService.UserId!.Value;
        var project = await _projectService.CreateProjectAsync(dto, userId);
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id },
            ApiResponse<ProjectDto>.SuccessResponse(project, "Layihə uğurla yaradıldı."));
    }

    /// <summary>Layihəni yeniləmək</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var userId = _currentUserService.UserId!.Value;
        var project = await _projectService.UpdateProjectAsync(id, dto, userId, IsAdmin);
        return Ok(ApiResponse<ProjectDto>.SuccessResponse(project, "Layihə uğurla yeniləndi."));
    }

    /// <summary>Layihəni silmək</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse>> DeleteProject(Guid id)
    {
        var userId = _currentUserService.UserId!.Value;
        await _projectService.DeleteProjectAsync(id, userId, IsAdmin);
        return Ok(ApiResponse.SuccessResponse("Layihə uğurla silindi."));
    }
}
