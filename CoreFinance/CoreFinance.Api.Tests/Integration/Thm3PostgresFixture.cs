using System.Text;
using CoreFinance.Api.Configuration;
using CoreFinance.Application.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Tests.Shared.Helpers;

namespace CoreFinance.Api.Tests.Integration;

/// <summary>
/// An actual application host with its own migrated PostgreSQL database and gateway-issued bearer tokens.
/// Only the API business calendar is fixed; authentication, hosted timers and worker calendars use the system clock.
/// </summary>
internal sealed class Thm3PostgresFixture : IAsyncDisposable
{
    // Each test owns its database, but two genuine gateway actors can be reused across the test process.
    // Per-case logins otherwise exceed the unchanged Local Identity route's 50 requests/minute limit.
    private static readonly object GatewayActorLock = new();
    private static readonly Task<string>?[] GatewayActors = new Task<string>?[2];
    private readonly string _databaseName = $"thm3_{Guid.NewGuid():N}";
    private readonly string _administrationConnectionString;
    private readonly string _rabbitHost;
    private readonly string _rabbitUsername;
    private readonly string _rabbitPassword;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly IConfiguration _configuration;
    private readonly DbContextOptions<CoreFinanceDbContext> _databaseOptions;
    private readonly List<WebApplicationFactory<Program>> _factories = [];
    private bool _databaseCreated;
    private bool _disposed;
    private int _gatewayActorIndex;

    private Thm3PostgresFixture(DateTimeOffset? businessNow)
    {
        var postgres = new NpgsqlConnectionStringBuilder(ExternalBackend.Require("TIHOMO_TEST_PG"));
        _administrationConnectionString = new NpgsqlConnectionStringBuilder(postgres.ConnectionString)
        {
            Database = "postgres",
            Pooling = false
        }.ConnectionString;
        postgres.Database = _databaseName;
        postgres.CommandTimeout = 90;
        ConnectionString = postgres.ConnectionString;

        var rabbit = new Uri(ExternalBackend.Require("TIHOMO_TEST_RABBITMQ"), UriKind.Absolute);
        if (rabbit.Scheme != "amqp" || rabbit.Port is not (-1 or 5672) || rabbit.AbsolutePath != "/")
            throw new InvalidOperationException(
                "TIHOMO_TEST_RABBITMQ must identify the actual application's AMQP port 5672 and root virtual host.");
        var credentials = rabbit.UserInfo.Split(':', 2);
        if (credentials.Length != 2)
            throw new InvalidOperationException("TIHOMO_TEST_RABBITMQ must contain the runner's RabbitMQ credentials.");
        _rabbitHost = rabbit.Host;
        _rabbitUsername = Uri.UnescapeDataString(credentials[0]);
        _rabbitPassword = Uri.UnescapeDataString(credentials[1]);
        _jwtSecret = ExternalBackend.Require("TIHOMO_TEST_JWT_SECRET");
        _jwtIssuer = ExternalBackend.Require("TIHOMO_TEST_JWT_ISSUER");
        _jwtAudience = ExternalBackend.Require("TIHOMO_TEST_JWT_AUDIENCE");
        ExternalBackend.Require("TIHOMO_BACKEND_URLS");

        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"ConnectionStrings:{CoreFinanceDbContext.DEFAULT_CONNECTION_STRING}"] = ConnectionString
        }).Build();
        _databaseOptions = new DbContextOptionsBuilder<CoreFinanceDbContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .Options;
        Today = new BusinessCalendar(businessNow is { } now ? new FixedBusinessClock(now) : TimeProvider.System).Today;
        Factory = CreateFactory(false, businessNow);
    }

    public string ConnectionString { get; }
    public DateTime Today { get; }
    public WebApplicationFactory<Program> Factory { get; }

    public static async Task<Thm3PostgresFixture> CreateAsync(DateTimeOffset? businessNow = null)
    {
        // Match the actual application's timestamp-without-time-zone compatibility setting.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var fixture = new Thm3PostgresFixture(businessNow);
        try
        {
            await using (var administration = new NpgsqlConnection(fixture._administrationConnectionString))
            {
                await administration.OpenAsync();
                await using var create = new NpgsqlCommand($"CREATE DATABASE \"{fixture._databaseName}\"", administration);
                await create.ExecuteNonQueryAsync();
                fixture._databaseCreated = true;
            }

            await using (var database = fixture.NewDbContext())
                await database.Database.MigrateAsync();

            // Starting the real host also requires the runner's actual RabbitMQ, without sending any HTTP request.
            _ = fixture.Factory.Services;
            return fixture;
        }
        catch
        {
            await fixture.DisposeAsync();
            throw;
        }
    }

    public Task<string> LoginTokenAsync(string prefix)
    {
        lock (GatewayActorLock)
        {
            var actorIndex = _gatewayActorIndex++;
            return GatewayActors[actorIndex] ??=
                ExternalBackend.LoginThroughGatewayAsync(ExternalBackend.NewUser(prefix));
        }
    }

    public async Task<BackendFinanceClient> LoginAsync(string prefix)
    {
        var token = await LoginTokenAsync(prefix);
        var http = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        http.Timeout = TimeSpan.FromSeconds(90);
        try
        {
            return BackendFinanceClient.Authenticate(http, token);
        }
        catch
        {
            http.Dispose();
            throw;
        }
    }

    public async Task<T> ReadAsync<T>(Func<CoreFinanceDbContext, Task<T>> read)
    {
        await using var database = NewDbContext();
        return await read(database);
    }

    public async Task<int> ExecuteSqlAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = await OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync()
    {
        var connection = new NpgsqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task<T> WithOwnerAsync<T>(Guid owner, Func<IServiceProvider, Task<T>> operation)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<CurrentUserContext>().InitializeBackgroundOwner(owner);
        return await operation(scope.ServiceProvider);
    }

    public async Task WithOwnerAsync(Guid owner, Func<IServiceProvider, Task> operation)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<CurrentUserContext>().InitializeBackgroundOwner(owner);
        await operation(scope.ServiceProvider);
    }

    public Task<WebApplicationFactory<Program>> StartWorkerAsync()
    {
        var worker = CreateFactory(true, null);
        _ = worker.Services;
        return Task.FromResult(worker);
    }

    private CoreFinanceDbContext NewDbContext() => new(_databaseOptions, _configuration);

    private WebApplicationFactory<Program> CreateFactory(bool enableWorker, DateTimeOffset? businessNow)
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{CoreFinanceDbContext.DEFAULT_CONNECTION_STRING}"] = ConnectionString,
                    ["RabbitMQ:Host"] = _rabbitHost,
                    ["RabbitMQ:Username"] = _rabbitUsername,
                    ["RabbitMQ:Password"] = _rabbitPassword,
                    ["JwtSettings:SecretKey"] = _jwtSecret,
                    ["JwtSettings:Issuer"] = _jwtIssuer,
                    ["JwtSettings:Audience"] = _jwtAudience,
                    ["DbSettingOptions:EnableSensitiveDataLogging"] = "false",
                    ["RecurringGeneration:Enabled"] = enableWorker.ToString(),
                    // The production bearer event logs headers at Information. Never emit bearer tokens in fixtures.
                    ["Serilog:MinimumLevel:Default"] = "Warning",
                    ["Serilog:MinimumLevel:Override:Microsoft"] = "Warning",
                    ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore.Authentication.JwtBearer"] = "Warning"
                }));
            builder.ConfigureServices(services =>
            {
                // Testing skips the production DbContext registration, not its repositories, UOW or services.
                services.AddDbContext<CoreFinanceDbContext>(options => options
                    .UseNpgsql(ConnectionString)
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging(false));
                services.PostConfigure<RecurringGenerationSettings>(settings => settings.Enabled = enableWorker);
                services.PostConfigure<MassTransitHostOptions>(options =>
                {
                    options.WaitUntilStarted = true;
                    options.StartTimeout = TimeSpan.FromSeconds(30);
                    options.StopTimeout = TimeSpan.FromSeconds(30);
                });
                if (businessNow is { } now)
                {
                    services.RemoveAll<BusinessCalendar>();
                    services.AddScoped(_ => new BusinessCalendar(new FixedBusinessClock(now)));
                }

                // Existing unit fixtures mutate JwtSettings process variables before Program reads configuration.
                // Override only the actual Bearer scheme's validation options, after every production registration.
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidIssuer = _jwtIssuer,
                        ValidAudience = _jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret)),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });
            });
        });
        _factories.Add(factory);
        return factory;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        List<Exception> failures = [];
        for (var index = _factories.Count - 1; index >= 0; index--)
        {
            try
            {
                await _factories[index].DisposeAsync();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        if (_databaseCreated)
        {
            try
            {
                using var ownPool = new NpgsqlConnection(ConnectionString);
                NpgsqlConnection.ClearPool(ownPool);
                await using var administration = new NpgsqlConnection(_administrationConnectionString);
                await administration.OpenAsync();
                // The name is generated privately by this fixture; never target a runner or default database.
                await using var drop = new NpgsqlCommand(
                    $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", administration);
                await drop.ExecuteNonQueryAsync();
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }

        if (failures.Count > 0)
            throw new AggregateException("Could not dispose this fixture's application/database resources.", failures);
    }

    private sealed class FixedBusinessClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
