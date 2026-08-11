using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IRoleRepository : IBaseRepository<Role, Guid>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsNameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> IsNameExistsAsync(string name, Guid excludeRoleId, CancellationToken cancellationToken = default);
    Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task RemoveUserFromRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
}