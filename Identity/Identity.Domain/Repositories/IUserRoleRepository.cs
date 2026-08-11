using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IUserRoleRepository : IBaseRepository<UserRole, Guid>
{
    Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task DeleteByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}