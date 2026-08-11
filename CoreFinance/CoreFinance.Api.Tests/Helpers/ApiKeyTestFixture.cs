using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace CoreFinance.Api.Tests.Helpers;

/// <summary>
///     Test fixture for API Key integration testing (EN)<br/>
///     Test fixture cho API Key integration testing (VI)
/// </summary>
public class ApiKeyTestFixture : IAsyncDisposable
{
    public WebApplicationFactory<Program> Factory { get; }
    public HttpClient Client { get; }
    public string TestDatabaseName { get; }
    public ITestOutputHelper? Output { get; set; }

    // Test data
    public Guid TestUserId { get; }
    public string TestApiKey { get; }

    public ApiKeyTestFixture()
    {
        TestDatabaseName = $"TestCoreFinanceDb_{Guid.CreateVersion7():N}";
        TestUserId = Guid.CreateVersion7();
        TestApiKey = "tihomo_test_api_key_12345678901234567890";

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                // Add minimal configuration for testing
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["CorsOptions:PolicyName"] = "TestCorsPolicy",
                        ["CorsOptions:AllowedOrigins:0"] = "*",
                        ["JwtSettings:SecretKey"] = "test_secret_key_for_testing_minimum_32_characters",
                        ["JwtSettings:Issuer"] = "test_issuer",
                        ["JwtSettings:Audience"] = "test_audience",
                        ["ConnectionStrings:DefaultConnection"] = "InMemory"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Add test database (PostgreSQL registration is skipped in Testing environment)
                    services.AddDbContext<CoreFinanceDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(TestDatabaseName);
                        options.EnableSensitiveDataLogging();
                    });

                    // Override logging for tests
                    services.AddLogging();
                });
            });

        Client = Factory.CreateClient();
        
        // Set default API Key authentication
        Client.DefaultRequestHeaders.Add("X-API-Key", TestApiKey);
    }

    /// <summary>
    ///     Initialize test database with sample data (EN)<br/>
    ///     Khởi tạo test database với sample data (VI)
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Add sample test data if needed
        await SeedTestDataAsync(context);
        
        Output?.WriteLine($"Test database initialized: {TestDatabaseName}");
    }

    /// <summary>
    ///     Seed test data into database (EN)<br/>
    ///     Seed test data vào database (VI)
    /// </summary>
    private async Task SeedTestDataAsync(CoreFinanceDbContext context)
    {
        // Add sample accounts, transactions, etc. for testing
        // This can be customized based on test requirements
        
        var sampleAccount = new CoreFinance.Domain.Entities.Account
        {
            Id = Guid.CreateVersion7(),
            UserId = TestUserId,
            Name = "Sample Test Account",
            Type = AccountType.Bank,
            Currency = "USD",
            InitialBalance = 1000.00m,
            CurrentBalance = 1000.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Accounts.Add(sampleAccount);
        await context.SaveChangesAsync();
        
        Output?.WriteLine($"Seeded sample account: {sampleAccount.Name}");
    }

    /// <summary>
    ///     Reset database to clean state (EN)<br/>
    ///     Reset database về trạng thái sạch (VI)
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync(context);
        
        Output?.WriteLine("Test database reset completed");
    }

    /// <summary>
    ///     Create a new HTTP client with different configuration (EN)<br/>
    ///     Tạo HTTP client mới với cấu hình khác (VI)
    /// </summary>
    public HttpClient CreateClient(string? apiKey = null)
    {
        var client = Factory.CreateClient();
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }
        
        return client;
    }

    /// <summary>
    ///     Create HTTP client without authentication (EN)<br/>
    ///     Tạo HTTP client không có authentication (VI)
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            // Clean up test database
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CoreFinanceDbContext>();
            await context.Database.EnsureDeletedAsync();
            
            Client.Dispose();
            Factory.Dispose();
            
            Output?.WriteLine($"Test fixture cleanup completed for database: {TestDatabaseName}");
        }
        catch (Exception ex)
        {
            Output?.WriteLine($"Error during test fixture cleanup: {ex.Message}");
        }
    }
}