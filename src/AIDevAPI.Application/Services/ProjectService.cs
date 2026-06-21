using AIDevAPI.Application.Common;
using AIDevAPI.Application.Common.Exceptions;
using AIDevAPI.Application.DTOs.Project;
using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Application.Interfaces.Services;
using AIDevAPI.Domain.Entities;

namespace AIDevAPI.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(PaginationParams pagination, Guid? ownerId, bool isAdmin)
    {
        var query = _unitOfWork.Projects.Query();

        if (!isAdmin && ownerId.HasValue)
            query = query.Where(p => p.OwnerId == ownerId.Value);

        if (!string.IsNullOrWhiteSpace(pagination.SearchTerm))
        {
            var term = pagination.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));
        }

        var totalCount = query.Count();

        var entities = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var items = entities.Select(MapToDto).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = items,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectDto> GetProjectByIdAsync(Guid id, Guid requesterId, bool isAdmin)
    {
        var project = await _unitOfWork.Projects.GetByIdWithOwnerAsync(id);
        if (project == null || project.IsDeleted)
            throw new NotFoundException("Layihə", id);

        if (!isAdmin && project.OwnerId != requesterId)
            throw new ForbiddenException("Bu layihəyə baxmaq üçün icazəniz yoxdur.");

        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid ownerId)
    {
        var project = new AIProject
        {
            Name = dto.Name,
            Description = dto.Description,
            Status = dto.Status,
            Technology = dto.Technology,
            ProgressPercentage = dto.ProgressPercentage,
            OwnerId = ownerId
        };

        await _unitOfWork.Projects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Projects.GetByIdWithOwnerAsync(project.Id);
        return MapToDto(created!);
    }

    public async Task<ProjectDto> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid requesterId, bool isAdmin)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null || project.IsDeleted)
            throw new NotFoundException("Layihə", id);

        if (!isAdmin && project.OwnerId != requesterId)
            throw new ForbiddenException("Bu layihəni dəyişdirmək üçün icazəniz yoxdur.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) project.Name = dto.Name;
        if (dto.Description != null) project.Description = dto.Description;
        if (dto.Status.HasValue) project.Status = dto.Status.Value;
        if (dto.Technology != null) project.Technology = dto.Technology;
        if (dto.ProgressPercentage.HasValue) project.ProgressPercentage = dto.ProgressPercentage.Value;
        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Projects.GetByIdWithOwnerAsync(id);
        return MapToDto(updated!);
    }

    public async Task DeleteProjectAsync(Guid id, Guid requesterId, bool isAdmin)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null || project.IsDeleted)
            throw new NotFoundException("Layihə", id);

        if (!isAdmin && project.OwnerId != requesterId)
            throw new ForbiddenException("Bu layihəni silmək üçün icazəniz yoxdur.");

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ProjectDto MapToDto(AIProject p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Status = p.Status,
        Technology = p.Technology,
        ProgressPercentage = p.ProgressPercentage,
        OwnerId = p.OwnerId,
        OwnerName = p.Owner != null ? $"{p.Owner.FirstName} {p.Owner.LastName}".Trim() : string.Empty,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
