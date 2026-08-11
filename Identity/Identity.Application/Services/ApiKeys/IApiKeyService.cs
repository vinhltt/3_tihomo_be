using Identity.Contracts;

namespace Identity.Application.Services.ApiKeys;

public interface IApiKeyService
{
    Task<CreateApiKeyResponse> CreateApiKeyAsync(Guid userId, CreateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiKeyResponse> GetApiKeyByIdAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApiKeyResponse>> GetUserApiKeysAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ApiKeyResponse> UpdateApiKeyAsync(Guid apiKeyId, UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default);

    Task RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    Task DeleteApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
    Task<VerifyApiKeyResponse> VerifyApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
}