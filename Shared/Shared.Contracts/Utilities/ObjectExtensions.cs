using System.Text.Json;

namespace Shared.Contracts.Utilities;

/// <summary>
/// Extension methods for object parsing and string conversion
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Safely converts an object to string representation
    /// </summary>
    /// <param name="obj">The object to convert</param>
    /// <returns>String representation of the object</returns>
    public static string TryParseToString(this object? obj)
    {
        try
        {
            return obj switch
            {
                null => "null",
                string str => str,
                _ => JsonSerializer.Serialize(obj, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                })
            };
        }
        catch
        {
            return obj?.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Safely converts a collection to string representation
    /// </summary>
    /// <typeparam name="T">Type of items in collection</typeparam>
    /// <param name="collection">The collection to convert</param>
    /// <returns>String representation of the collection</returns>
    public static string TryParseToString<T>(this IEnumerable<T>? collection)
    {
        try
        {
            if (collection == null) return "null";
            
            var items = collection.ToList();
            return $"[{items.Count} items]";
        }
        catch
        {
            return "collection";
        }
    }

    /// <summary>
    /// Safely converts an array to string representation
    /// </summary>
    /// <typeparam name="T">Type of items in array</typeparam>
    /// <param name="array">The array to convert</param>
    /// <returns>String representation of the array</returns>
    public static string TryParseToString<T>(this T[]? array)
    {
        try
        {
            if (array == null) return "null";
            return $"[{array.Length} items]";
        }
        catch
        {
            return "array";
        }
    }
}
