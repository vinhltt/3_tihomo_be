using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Contracts.Extensions;

/// <summary>
/// Entity Framework Core extension methods
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Configure database context with PostgreSQL
    /// </summary>
    /// <typeparam name="TContext">Database context type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="connectionStringName">Connection string name</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddPostgreSqlContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseNodaTime();
                npgsqlOptions.EnableRetryOnFailure();
            });
            
            options.UseSnakeCaseNamingConvention();
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        return services;
    }

    /// <summary>
    /// Configure database context for testing with in-memory database
    /// </summary>
    /// <typeparam name="TContext">Database context type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="databaseName">In-memory database name</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInMemoryContext<TContext>(
        this IServiceCollection services,
        string databaseName)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseInMemoryDatabase(databaseName);
            options.EnableSensitiveDataLogging(true);
            options.EnableDetailedErrors(true);
        });

        return services;
    }
}

/// <summary>
/// String extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Convert string to snake_case
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Snake case string</returns>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return System.Text.RegularExpressions.Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }

    /// <summary>
    /// Check if string is null or empty
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>True if null or empty</returns>
    public static bool IsNullOrEmpty(this string? input)
    {
        return string.IsNullOrEmpty(input);
    }

    /// <summary>
    /// Check if string is null, empty or whitespace
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>True if null, empty or whitespace</returns>
    public static bool IsNullOrWhiteSpace(this string? input)
    {
        return string.IsNullOrWhiteSpace(input);
    }
}
