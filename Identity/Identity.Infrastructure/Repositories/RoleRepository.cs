using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class RoleRepository(IdentityDbContext context) : BaseRepository<Role, Guid>(context), IRoleRepository
{
    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<bool> IsNameExistsAsync(string name, Guid excludeRoleId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.Name == name && r.Id != excludeRoleId, cancellationToken);
    }

    public async Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        };

        Context.UserRoles.Add(userRole);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveUserFromRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var userRole = await Context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

        if (userRole != null)
        {
            Context.UserRoles.Remove(userRole);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await Context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }
}