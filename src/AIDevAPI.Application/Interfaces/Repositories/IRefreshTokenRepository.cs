using AIDevAPI.Domain.Entities;

namespace AIDevAPI.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
}
