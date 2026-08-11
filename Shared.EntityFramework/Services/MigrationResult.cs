namespace Shared.EntityFramework.Services;

/// <summary>
/// Result of database migration operation (EN)<br/>
/// Kết quả của operation migration database (VI)
/// </summary>
public class MigrationResult
{
    public bool Success { get; private set; }
    public string ServiceName { get; private set; }
    public bool PerformedMigration { get; private set; }
    public string? ErrorMessage { get; private set; }

    private MigrationResult(bool success, string serviceName, bool performedMigration, string? errorMessage)
    {
        Success = success;
        ServiceName = serviceName;
        PerformedMigration = performedMigration;
        ErrorMessage = errorMessage;
    }

    public static MigrationResult CreateSuccess(string serviceName, bool performedMigration)
    {
        return new MigrationResult(true, serviceName, performedMigration, null);
    }

    public static MigrationResult Failed(string serviceName, string errorMessage)
    {
        return new MigrationResult(false, serviceName, false, errorMessage);
    }
}
