using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Shared.EntityFramework.Services;

/// <summary>
/// Database migration service với distributed locking (EN)<br/>
/// Service migration database với distributed locking (VI)
/// </summary>
public class DatabaseMigrationService
{
    private const int MAX_WAIT_SECONDS = 30;
    private const int WAIT_INTERVAL_MS = 1000;

    /// <summary>
    /// Migrate database with PostgreSQL advisory lock (EN)<br/>
    /// Migrate database với PostgreSQL advisory lock (VI)
    /// </summary>
    public async Task<MigrationResult> MigrateWithLockAsync(
        DbContext context,
        string serviceName,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(serviceName);
        ArgumentNullException.ThrowIfNull(logger);

        var lockId = GenerateLockId(serviceName);

        try
        {
            // Ensure database exists in Development
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == "Development")
            {
                await EnsureDatabaseExistsAsync(context, serviceName, logger, cancellationToken);
            }

            // Try to acquire advisory lock
            var lockAcquired = await TryAcquireLockAsync(context, lockId, cancellationToken);

            if (lockAcquired)
            {
                logger.LogInformation("🔒 Service {Service} acquired lock {LockId}. Performing migration...",
                    serviceName, lockId);
                return await PerformMigrationAsync(context, serviceName, logger, lockId, cancellationToken);
            }

            logger.LogInformation("⏳ Service {Service} could not acquire lock {LockId}. Waiting for migration...",
                serviceName, lockId);
            return await WaitForMigrationAsync(context, serviceName, logger, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Migration failed for service {Service}", serviceName);
            return MigrationResult.Failed(serviceName, ex.Message);
        }
    }

    private async Task EnsureDatabaseExistsAsync(
        DbContext context,
        string serviceName,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(serviceName);
        ArgumentNullException.ThrowIfNull(logger);

        var npgsqlConnection = context.Database.GetDbConnection() as NpgsqlConnection;

        if (npgsqlConnection == null)
        {
            throw new InvalidOperationException("Database connection must be an NpgsqlConnection");
        }

        // Get database name from connection string
        var databaseName = npgsqlConnection.Database;

        // Security: Validate database name to prevent SQL injection
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database name cannot be empty");
        }

        // PostgreSQL database names must: start with letter/underscore, contain only letters/numbers/underscores
        // Also limit length to 63 characters (PostgreSQL limit)
        if (!System.Text.RegularExpressions.Regex.IsMatch(databaseName, "^[a-zA-Z_][a-zA-Z0-9_]{0,62}$"))
        {
            throw new InvalidOperationException(
                $"Invalid database name '{databaseName}'. Must start with letter/underscore and contain only letters, numbers, and underscores (max 63 chars).");
        }

        // Connect to 'postgres' database to check/create target database
        var builder = new NpgsqlConnectionStringBuilder(npgsqlConnection.ConnectionString)
        {
            Database = "postgres"
        };

        await using var tempConnection = new NpgsqlConnection(builder.ToString());
        await tempConnection.OpenAsync(cancellationToken);

        // Check if database exists
        await using var checkCommand = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @name)", tempConnection);
        checkCommand.Parameters.Add(new NpgsqlParameter("name", databaseName));

        var exists = Convert.ToBoolean(await checkCommand.ExecuteScalarAsync(cancellationToken));

        if (!exists)
        {
            logger.LogInformation("🔧 Service {Service} - Database {Database} does not exist. Creating...",
                serviceName, databaseName);

            // Safe: databaseName is validated against regex pattern
            await using var createCommand = new NpgsqlCommand(
                $"CREATE DATABASE \"{databaseName}\" ENCODING 'UTF8'", tempConnection);
            await createCommand.ExecuteNonQueryAsync(cancellationToken);

            logger.LogInformation("✅ Service {Service} - Database {Database} created successfully",
                serviceName, databaseName);
        }
        else
        {
            logger.LogInformation("✅ Service {Service} - Database {Database} already exists",
                serviceName, databaseName);
        }
    }

    private async Task<bool> TryAcquireLockAsync(DbContext context, long lockId, CancellationToken cancellationToken)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);

        var npgsqlConnection = context.Database.GetDbConnection() as NpgsqlConnection;

        if (npgsqlConnection == null)
        {
            throw new InvalidOperationException("Database connection must be an NpgsqlConnection");
        }

        if (npgsqlConnection.State != ConnectionState.Open)
        {
            await npgsqlConnection.OpenAsync(cancellationToken);
        }

        await using var command = new NpgsqlCommand("SELECT pg_try_advisory_lock(@lockId)", npgsqlConnection);
        command.Parameters.Add(new NpgsqlParameter("lockId", lockId));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToBoolean(result);
    }

    private async Task<MigrationResult> PerformMigrationAsync(
        DbContext context,
        string serviceName,
        ILogger logger,
        long lockId,
        CancellationToken cancellationToken)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(serviceName);
        ArgumentNullException.ThrowIfNull(logger);

        try
        {
            // Check pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingList = pendingMigrations.ToList();

            if (pendingList.Count > 0)
            {
                logger.LogInformation("📋 Service {Service} found {Count} pending migrations: {Migrations}",
                    serviceName, pendingList.Count, string.Join(", ", pendingList));

                // Apply migrations
                await context.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("✅ Service {Service} applied {Count} migrations successfully",
                    serviceName, pendingList.Count);
            }
            else
            {
                logger.LogInformation("✅ Service {Service} - No pending migrations", serviceName);
            }

            // Release lock
            await ReleaseLockAsync(context, lockId, cancellationToken);
            logger.LogInformation("🔓 Service {Service} released lock {LockId}", serviceName, lockId);

            return MigrationResult.CreateSuccess(serviceName, performedMigration: pendingList.Count > 0);
        }
        catch (Exception)
        {
            // Release lock on error
            await ReleaseLockAsync(context, lockId, cancellationToken);
            throw;
        }
    }

    private async Task<MigrationResult> WaitForMigrationAsync(
        DbContext context,
        string serviceName,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(serviceName);
        ArgumentNullException.ThrowIfNull(logger);

        var attempts = 0;

        while (attempts < MAX_WAIT_SECONDS)
        {
            await Task.Delay(WAIT_INTERVAL_MS, cancellationToken);
            attempts++;

            // Check if migrations are applied
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);

            if (!pendingMigrations.Any())
            {
                logger.LogInformation("✅ Service {Service} - Migration completed by another service after {Attempts}s",
                    serviceName, attempts);
                return MigrationResult.CreateSuccess(serviceName, performedMigration: false);
            }

            logger.LogInformation("⏳ Service {Service} waiting... ({Attempts}/{MaxSeconds}s)",
                serviceName, attempts, MAX_WAIT_SECONDS);
        }

        var errorMessage = $"Timeout waiting for migration after {MAX_WAIT_SECONDS}s";
        logger.LogError("❌ {ErrorMessage}", errorMessage);
        return MigrationResult.Failed(serviceName, errorMessage);
    }

    private async Task ReleaseLockAsync(DbContext context, long lockId, CancellationToken cancellationToken)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);

        var npgsqlConnection = context.Database.GetDbConnection() as NpgsqlConnection;

        if (npgsqlConnection == null)
        {
            throw new InvalidOperationException("Database connection must be an NpgsqlConnection");
        }

        if (npgsqlConnection.State != ConnectionState.Open)
        {
            await npgsqlConnection.OpenAsync(cancellationToken);
        }

        using var command = new NpgsqlCommand("SELECT pg_advisory_unlock(@lockId)", npgsqlConnection);
        command.Parameters.Add(new NpgsqlParameter("lockId", lockId));

        await command.ExecuteScalarAsync(cancellationToken);
    }

    private long GenerateLockId(string serviceName)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(serviceName);

        return Math.Abs(serviceName.GetHashCode() % 2147483647L);
    }
}
