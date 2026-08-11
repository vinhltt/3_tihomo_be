using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRoleRepository(IdentityDbContext context)
    : BaseRepository<UserRole, Guid>(context), IUserRoleRepository
{
    public async Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task DeleteByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await GetByUserAndRoleAsync(userId, roleId, cancellationToken);
        if (userRole != null)
        {
            Context.UserRoles.Remove(userRole);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}