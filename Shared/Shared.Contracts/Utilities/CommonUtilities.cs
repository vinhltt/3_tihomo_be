namespace Shared.Contracts.Utilities;

/// <summary>
/// Common utility functions for string operations
/// </summary>
public static class StringUtilities
{
    /// <summary>
    /// Check if string is null or empty
    /// </summary>
    public static bool IsNullOrEmpty(string? value) => string.IsNullOrEmpty(value);
    
    /// <summary>
    /// Check if string is null, empty, or whitespace
    /// </summary>
    public static bool IsNullOrWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value);
    
    /// <summary>
    /// Safe trim operation
    /// </summary>
    public static string? SafeTrim(string? value) => value?.Trim();
}

/// <summary>
/// Common utility functions for date operations
/// </summary>
public static class DateUtilities
{
    /// <summary>
    /// Get start of day
    /// </summary>
    public static DateTime StartOfDay(DateTime date) => date.Date;
    
    /// <summary>
    /// Get end of day
    /// </summary>
    public static DateTime EndOfDay(DateTime date) => date.Date.AddDays(1).AddTicks(-1);
}

/// <summary>
/// Common utility functions for validation
/// </summary>
public static class ValidationUtilities
{
    /// <summary>
    /// Validate if ID is not default value
    /// </summary>
    public static bool IsValidId<T>(T id) where T : IEquatable<T>
        => !id.Equals(default(T));
    
    /// <summary>
    /// Validate email format
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
