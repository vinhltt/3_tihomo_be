namespace Identity.Application.Common.Exceptions;

/// <summary>
/// API Key related exceptions - Các ngoại lệ liên quan đến API Key (EN)<br/>
/// Các ngoại lệ liên quan đến API Key (VI)
/// </summary>
public class ApiKeyException : Exception
{
    public ApiKeyException(string message) : base(message) { }
    public ApiKeyException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when API key limit is exceeded - Ngoại lệ khi vượt quá giới hạn API key (EN)<br/>
/// Ngoại lệ khi vượt quá giới hạn API key (VI)
/// </summary>
public class ApiKeyLimitExceededException : ApiKeyException
{
    public int CurrentCount { get; }
    public int MaxAllowed { get; }

    public ApiKeyLimitExceededException(int currentCount, int maxAllowed) 
        : base($"API key limit exceeded. Current: {currentCount}, Max allowed: {maxAllowed}")
    {
        CurrentCount = currentCount;
        MaxAllowed = maxAllowed;
    }
}

/// <summary>
/// Exception thrown when API key is invalid or expired - Ngoại lệ khi API key không hợp lệ hoặc hết hạn (EN)<br/>
/// Ngoại lệ khi API key không hợp lệ hoặc hết hạn (VI)
/// </summary>
public class InvalidApiKeyException : ApiKeyException
{
    public string? ApiKeyPrefix { get; }
    public string Reason { get; }

    public InvalidApiKeyException(string reason, string? apiKeyPrefix = null) 
        : base($"Invalid API key: {reason}")
    {
        Reason = reason;
        ApiKeyPrefix = apiKeyPrefix;
    }
}

/// <summary>
/// Exception thrown when API key validation fails - Ngoại lệ khi xác thực API key thất bại (EN)<br/>
/// Ngoại lệ khi xác thực API key thất bại (VI)
/// </summary>
public class ApiKeyValidationException : ApiKeyException
{
    public string ValidationStep { get; }
    public Dictionary<string, object> ValidationContext { get; }

    public ApiKeyValidationException(string validationStep, string message, Dictionary<string, object>? context = null) 
        : base(message)
    {
        ValidationStep = validationStep;
        ValidationContext = context ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Exception thrown when rate limit is exceeded - Ngoại lệ khi vượt quá giới hạn tần suất (EN)<br/>
/// Ngoại lệ khi vượt quá giới hạn tần suất (VI)
/// </summary>
public class RateLimitExceededException : ApiKeyException
{
    public Guid ApiKeyId { get; }
    public int CurrentUsage { get; }
    public int Limit { get; }
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(Guid apiKeyId, int currentUsage, int limit, TimeSpan retryAfter) 
        : base($"Rate limit exceeded for API key. Current: {currentUsage}, Limit: {limit}")
    {
        ApiKeyId = apiKeyId;
        CurrentUsage = currentUsage;
        Limit = limit;
        RetryAfter = retryAfter;
    }
}