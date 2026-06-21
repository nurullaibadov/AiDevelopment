using AIDevAPI.Domain.Common;
using AIDevAPI.Domain.Enums;

namespace AIDevAPI.Domain.Entities;

public class AIProject : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public string? Technology { get; set; }
    public double ProgressPercentage { get; set; }

    public Guid OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;
}
