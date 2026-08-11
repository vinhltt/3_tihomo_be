using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class RefreshTokenRepository(IdentityDbContext context)
    : BaseRepository<RefreshToken, Guid>(context), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, string revokedBy,
        CancellationToken cancellationToken = default)
    {
        var tokens = await DbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedBy = revokedBy;
            token.RevokedAt = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeTokenAsync(string token, string revokedBy, CancellationToken cancellationToken = default)
    {
        var refreshToken = await DbSet
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedBy = revokedBy;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await DbSet
            .Where(rt => rt.ExpiresAt <= DateTime.UtcNow && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in expiredTokens)
        {
            token.IsRevoked = true;
            token.RevokedBy = "system";
            token.RevokedAt = DateTime.UtcNow;
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}