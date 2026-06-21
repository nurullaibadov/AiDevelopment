using AIDevAPI.Domain.Entities;

namespace AIDevAPI.Application.Interfaces.Repositories;

public interface IProjectRepository : IGenericRepository<AIProject>
{
    Task<AIProject?> GetByIdWithOwnerAsync(Guid id);
}
