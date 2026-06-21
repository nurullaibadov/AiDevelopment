namespace AIDevAPI.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int LockedUsers { get; set; }
    public int TotalRoles { get; set; }
    public int TotalProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int InProgressProjects { get; set; }
    public int NewUsersLast7Days { get; set; }
    public int NewProjectsLast7Days { get; set; }
}
