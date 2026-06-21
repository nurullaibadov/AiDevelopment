using AIDevAPI.Application.Common;
using AIDevAPI.Application.DTOs.Project;

namespace AIDevAPI.Application.Interfaces.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(PaginationParams pagination, Guid? ownerId, bool isAdmin);
    Task<ProjectDto> GetProjectByIdAsync(Guid id, Guid requesterId, bool isAdmin);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid ownerId);
    Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid requesterId, bool isAdmin);
    Task DeleteProjectAsync(Guid id, Guid requesterId, bool isAdmin);
}
