using AIDevAPI.Domain.Entities;

namespace AIDevAPI.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync();
}
