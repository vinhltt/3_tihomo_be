using Identity.Domain.Enums;

namespace Identity.Domain.Dtos.ApiKeys;

public record CreateApiKeyRequest(
    string Name,
    List<string> Scopes,
    DateTime? ExpiresAt);

public record CreateApiKeyResponse(
    Guid Id,
    string Name,
    string ApiKey, // Only shown once!
    List<string> Scopes,
    DateTime CreatedAt,
    DateTime? ExpiresAt);

public record UpdateApiKeyRequest(
    string Name,
    List<string> Scopes,
    DateTime? ExpiresAt);

public record ApiKeyResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public int UsageCount { get; set; }
    public string? RawKey { get; set; } // Only populated during creation
}

public record VerifyApiKeyResponse
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public List<string> Scopes { get; set; } = [];
    public string Message { get; set; } = string.Empty;
}

public record ApiKeyDetailResponse(
    Guid Id,
    string Name,
    List<string> Scopes,
    ApiKeyStatus Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    int UsageCount);

public record ApiKeyListItem(
    Guid Id,
    string Name,
    ApiKeyStatus Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? LastUsedAt,
    int UsageCount);