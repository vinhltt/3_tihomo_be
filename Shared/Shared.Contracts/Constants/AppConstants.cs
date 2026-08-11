namespace Shared.Contracts.Constants;

/// <summary>
/// Application wide constants
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public const int DefaultPageSize = 20;
    
    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public const int MaxPageSize = 100;
    
    /// <summary>
    /// Default connection string name
    /// </summary>
    public const string DefaultConnectionString = "DefaultConnection";
    
    /// <summary>
    /// Default schema name
    /// </summary>
    public const string DefaultSchema = "public";
}

/// <summary>
/// Database related constants
/// </summary>
public static class DatabaseConstants
{
    /// <summary>
    /// Maximum length for short text fields
    /// </summary>
    public const int ShortTextLength = 50;
    
    /// <summary>
    /// Maximum length for medium text fields
    /// </summary>
    public const int MediumTextLength = 255;
    
    /// <summary>
    /// Maximum length for long text fields
    /// </summary>
    public const int LongTextLength = 1000;
    
    /// <summary>
    /// Maximum length for description fields
    /// </summary>
    public const int DescriptionLength = 2000;
}

/// <summary>
/// Caching related constants
/// </summary>
public static class CacheConstants
{
    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public const int DefaultCacheExpirationMinutes = 60;
    
    /// <summary>
    /// Short cache expiration in minutes
    /// </summary>
    public const int ShortCacheExpirationMinutes = 5;
    
    /// <summary>
    /// Long cache expiration in minutes
    /// </summary>
    public const int LongCacheExpirationMinutes = 1440; // 24 hours
}
