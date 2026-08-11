using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Repositories;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class ApiKeyRepository(IdentityDbContext context) : BaseRepository<ApiKey, Guid>(context), IApiKeyRepository
{
    public async Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ak => ak.User)
            .FirstOrDefaultAsync(ak => ak.KeyHash == keyHash, cancellationToken);
    }

    public async Task<IEnumerable<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(ak => ak.UserId == userId && ak.Status == ApiKeyStatus.Active)
            .OrderByDescending(ak => ak.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApiKey?> GetActiveKeyByHashAsync(string keyHash, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ak => ak.User)
            .FirstOrDefaultAsync(ak => ak.KeyHash == keyHash &&
                                       ak.Status == ApiKeyStatus.Active,
                cancellationToken);
    }

    public async Task UpdateLastUsedAsync(Guid apiKeyId, DateTime lastUsedAt,
        CancellationToken cancellationToken = default)
    {
        var apiKey = await DbSet.FindAsync([apiKeyId], cancellationToken);
        if (apiKey != null)
        {
            apiKey.LastUsedAt = lastUsedAt;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task IncrementUsageCountAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
    {
        var apiKey = await DbSet.FindAsync([apiKeyId], cancellationToken);
        if (apiKey != null)
        {
            apiKey.UsageCount++;
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<ApiKeyUsageLog>> GetUsageLogsAsync(Guid apiKeyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ApiKeyUsageLog>()
            .Where(log => log.ApiKeyId == apiKeyId && 
                         log.Timestamp >= startDate && 
                         log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync(cancellationToken);
    }
}