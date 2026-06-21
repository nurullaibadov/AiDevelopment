using AIDevAPI.Application.Interfaces.Repositories;
using AIDevAPI.Domain.Entities;
using AIDevAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIDevAPI.Infrastructure.Persistence.Repositories;

public class ProjectRepository : GenericRepository<AIProject>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override IQueryable<AIProject> Query() => base.Query().Include(p => p.Owner);

    public async Task<AIProject?> GetByIdWithOwnerAsync(Guid id)
        => await _dbSet.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id);
}
