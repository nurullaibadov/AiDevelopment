using System.ComponentModel.DataAnnotations;
using AIDevAPI.Domain.Enums;

namespace AIDevAPI.Application.DTOs.Project;

public class CreateProjectDto
{
    [Required(ErrorMessage = "Layihə adı mütləqdir")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıqlama mütləqdir")]
    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    public string? Technology { get; set; }

    [Range(0.0, 100.0)]
    public double ProgressPercentage { get; set; } = 0;
}
