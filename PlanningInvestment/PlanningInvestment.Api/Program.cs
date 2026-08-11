using Microsoft.EntityFrameworkCore;
using PlanningInvestment.Api;
using PlanningInvestment.Infrastructure;
using Shared.EntityFramework.Services;

async Task MigrateDatabaseWithLockAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    // Skip auto-migration in Production
    if (environment == "Production")
    {
        logger.LogWarning("⚠️ Running in Production. Auto-migration is DISABLED. Please run migrations manually.");
        logger.LogWarning("🔧 Use 'dotnet ef database update' to apply migrations before deployment.");
        return;
    }

    // Skip in Testing environment
    if (environment == "Testing")
    {
        logger.LogInformation("🧪 Running in Testing environment. Skipping auto-migration.");
        return;
    }

    logger.LogInformation("🔄 Starting database migration for PlanningInvestment service...");

    try
    {
        var context = services.GetRequiredService<PlanningInvestmentDbContext>();
        var migrationService = new DatabaseMigrationService();

        var result = await migrationService.MigrateWithLockAsync(
            context,
            "PlanningInvestment",
            logger);

        if (result.Success)
        {
            var status = result.PerformedMigration ? "performed migration" : "verified migrations";
            logger.LogInformation("✅ PlanningInvestment service started successfully - {Status}", status);
        }
        else
        {
            logger.LogError("❌ PlanningInvestment migration failed: {Error}", result.ErrorMessage);
            if (environment != "Production")
            {
                throw new InvalidOperationException($"Database migration failed: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ PlanningInvestment migration error: {Message}", ex.Message);
        if (environment != "Production")
        {
            throw;
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add database context
// Thêm ngữ cảnh cơ sở dữ liệu
builder.Services.AddDbContext<PlanningInvestmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(PlanningInvestmentDbContext.DEFAULT_CONNECTION_STRING))
           .UseSnakeCaseNamingConvention());

// Add health checks with DbContext
// Thêm kiểm tra sức khỏe với DbContext
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlanningInvestmentDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

// Add health check endpoint
app.MapHealthChecks("/health");

// ✅ Migrate database on startup
// Tự động migrate database khi khởi động
await MigrateDatabaseWithLockAsync(app);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

namespace PlanningInvestment.Api
{
    internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}