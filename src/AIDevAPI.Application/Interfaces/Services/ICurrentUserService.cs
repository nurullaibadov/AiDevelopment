namespace AIDevAPI.Application.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    string IpAddress { get; }
}
