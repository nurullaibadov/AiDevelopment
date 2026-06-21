using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Domain.Entities;
using AIDevAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIDevAPI.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
        => await _dbSet.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.Token == token);

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
        => await _dbSet
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
}
