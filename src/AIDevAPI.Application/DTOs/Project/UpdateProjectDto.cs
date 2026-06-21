using System.ComponentModel.DataAnnotations;
using AIDevAPI.Domain.Enums;

namespace AIDevAPI.Application.DTOs.Project;

public class UpdateProjectDto
{
    [MaxLength(150)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    public ProjectStatus? Status { get; set; }

    public string? Technology { get; set; }

    [Range(0.0, 100.0)]
    public double? ProgressPercentage { get; set; }
}
