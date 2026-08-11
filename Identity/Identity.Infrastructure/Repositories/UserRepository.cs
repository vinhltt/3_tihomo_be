using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class UserRepository(IdentityDbContext context) : BaseRepository<User, Guid>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.GoogleId == googleId, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailExistsAsync(string email, Guid excludeUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Email == email && u.Id != excludeUserId, cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, Guid excludeUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(u => u.Username == username && u.Id != excludeUserId, cancellationToken);
    }

    public async Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt,
        CancellationToken cancellationToken = default)
    {
        var user = await DbSet.FindAsync([userId], cancellationToken);
        if (user != null)
        {
            user.LastLoginAt = lastLoginAt;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public override async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FullName.Contains(search) ||
                                     u.Email.Contains(search) ||
                                     u.Username.Contains(search));

        return await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(string? search = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.FullName.Contains(search) ||
                                     u.Email.Contains(search) ||
                                     u.Username.Contains(search));

        return await query.CountAsync(cancellationToken);
    }
}