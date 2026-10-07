using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MoneyManagement.Application;
using MoneyManagement.Application.Services;
using MoneyManagement.Infrastructure;
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

    logger.LogInformation("🔄 Starting database migration for MoneyManagement service...");

    try
    {
        var context = services.GetRequiredService<MoneyManagementDbContext>();
        var migrationService = new DatabaseMigrationService();

        var result = await migrationService.MigrateWithLockAsync(
            context,
            "MoneyManagement",
            logger);

        if (result.Success)
        {
            var status = result.PerformedMigration ? "performed migration" : "verified migrations";
            logger.LogInformation("✅ MoneyManagement service started successfully - {Status}", status);
        }
        else
        {
            logger.LogError("❌ MoneyManagement migration failed: {Error}", result.ErrorMessage);
            if (environment != "Production")
            {
                throw new InvalidOperationException($"Database migration failed: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ MoneyManagement migration error: {Message}", ex.Message);
        if (environment != "Production")
        {
            throw;
        }
    }
}

var builder = WebApplication.CreateBuilder(args);

// JWT issued by Identity; the repository scopes reads by the NameIdentifier claim
var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured"),
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured"),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

// Add services to the container
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<BudgetService>();

// Add Application and Infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add HTTP context accessor for user context
builder.Services.AddHttpContextAccessor();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MoneyManagement API",
        Version = "v1",
        Description = "Personal Finance Management API for budgets, jars, and shared expenses"
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MoneyManagementDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MoneyManagement API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoint
app.MapHealthChecks("/health");

// ✅ Migrate database on startup
// Tự động migrate database khi khởi động
await MigrateDatabaseWithLockAsync(app);

app.Run();