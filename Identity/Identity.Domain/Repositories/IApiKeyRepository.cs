using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IApiKeyRepository : IBaseRepository<ApiKey, Guid>
{
    Task<ApiKey?> GetByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKey>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiKey?> GetActiveKeyByHashAsync(string keyHash, CancellationToken cancellationToken = default);
    Task UpdateLastUsedAsync(Guid apiKeyId, DateTime lastUsedAt, CancellationToken cancellationToken = default);
    Task IncrementUsageCountAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKeyUsageLog>> GetUsageLogsAsync(Guid apiKeyId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}