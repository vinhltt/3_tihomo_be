using System.Diagnostics;
using Ocelot.DependencyInjection;
using Ocelot.Gateway.Configuration;
using Ocelot.Gateway.Extensions;
using Ocelot.Gateway.Middleware;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Text.Json;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

JsonSerializerOptions options = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog(); // Add configuration
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
        .AddEnvironmentVariables();

    // TEMPORARY: Simplified config loading for debugging
    // Just load the Development config directly
    builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", false, true);

    // Read service ports configuration
    var servicePorts = builder.Configuration.GetSection("ServicePorts").Get<Dictionary<string, int>>() ??
                       [];

    // Read ocelot.json and replace environment variables with actual port values
    // var ocelotConfig = File.ReadAllText(Path.Combine(builder.Environment.ContentRootPath, "ocelot.json"));
    // foreach (var port in servicePorts) ocelotConfig = ocelotConfig.Replace($"${{{port.Key}}}", port.Value.ToString());

    // Write the processed ocelot configuration to a temporary file
    // var tempOcelotPath = Path.Combine(builder.Environment.ContentRootPath, "ocelot.processed.json");
    // File.WriteAllText(tempOcelotPath, ocelotConfig);

    // Add the processed ocelot configuration
    // builder.Configuration.AddJsonFile("ocelot.processed.json", false, true)
    //     .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", true, true);

    // Bind configuration sections
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
    var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ??
                       new CorsSettings();
    var rateLimitSettings = builder.Configuration.GetSection(RateLimitSettings.SectionName).Get<RateLimitSettings>() ??
                            new RateLimitSettings();
    var internalServiceSettings = builder.Configuration.GetSection(InternalServiceSettings.SectionName).Get<InternalServiceSettings>() ??
                            new InternalServiceSettings();

    // Configure settings as services
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(ApiKeySettings.SectionName));
    builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
    builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection(RateLimitSettings.SectionName));
    builder.Services.Configure<InternalServiceSettings>(builder.Configuration.GetSection(InternalServiceSettings.SectionName));
    builder.Services.Configure<ApiKeyToJwtSettings>(builder.Configuration.GetSection(ApiKeyToJwtSettings.SectionName));
    builder.Services.Configure<ServicePorts>(builder.Configuration.GetSection(ServicePorts.SectionName));

    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    // Add authentication and authorization
    builder.Services.AddAuthentication(jwtSettings);
    builder.Services.AddAuthorizationPolicies();

    // Add CORS
    builder.Services.AddCorsConfiguration(corsSettings);

    // Add health checks
    builder.Services.AddDownstreamHealthChecks(builder.Configuration);

    // Add memory cache
    builder.Services.AddMemoryCache();

    // Add HttpContextAccessor for delegating handlers
    builder.Services.AddHttpContextAccessor();

    // Add auth metrics service
    builder.Services.AddSingleton<Ocelot.Gateway.Services.AuthMetricsService>();

    // Add HttpClient for Identity service communication with resilience policies
    var apiKeyToJwtSettings = builder.Configuration.GetSection(ApiKeyToJwtSettings.SectionName).Get<ApiKeyToJwtSettings>() ?? new ApiKeyToJwtSettings();
    builder.Services.AddHttpClient("IdentityService", client =>
    {
        client.BaseAddress = new Uri(internalServiceSettings.IdentityService!); // Identity service URL (corrected port)
        client.Timeout = TimeSpan.FromMilliseconds(apiKeyToJwtSettings.TimeoutMs);
        client.DefaultRequestHeaders.Add("User-Agent", "TiHoMo-Gateway/1.0");
    })
    .AddPolicyHandler(ResilienceExtensions.GetRetryPolicy())
    .AddPolicyHandler(ResilienceExtensions.GetCircuitBreakerPolicy())
    .AddPolicyHandler(ResilienceExtensions.GetTimeoutPolicy(apiKeyToJwtSettings.TimeoutMs));

    // Note: CoreFinanceProxyMiddleware is added directly to pipeline, no DI registration needed
    
    // Add Ocelot without delegating handlers (using custom proxy middleware instead)
    builder.Services.AddOcelot(builder.Configuration);

    // ✅ Configure OtelSettings from appsettings.json
    // Cấu hình OtelSettings từ appsettings.json
    var otelSettings = builder.Configuration.GetSection("OtelSettings").Get<OtelSettings>() ?? new OtelSettings
    {
        ServiceName = "TiHoMo.Gateway",
        ServiceVersion = "1.0.0",
        ExporterOtlpEndpoint = "http://tempo:4317",
        TracesSampler = "traceidratio",
        TracesSamplerArg = "1.0"
    };

    // ✅ Configure ActivitySource for distributed tracing
    // Cấu hình ActivitySource cho distributed tracing
    var activitySource = new ActivitySource(otelSettings.ServiceName);
    builder.Services.AddSingleton(activitySource);

    // ✅ Add OpenTelemetry for comprehensive observability
    // Thêm OpenTelemetry cho observability toàn diện
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(otelSettings.ServiceName, otelSettings.ServiceVersion)
            .AddAttributes(otelSettings.GetResourceAttributesDictionary())
            .AddAttributes(new Dictionary<string, object>
            {
                ["service.namespace"] = "TiHoMo",
                ["service.instance.id"] = Environment.MachineName,
                ["deployment.environment"] = builder.Environment.EnvironmentName,
                ["service.type"] = "gateway"
            }))
        .WithTracing(tracing => tracing
            .SetSampler(new TraceIdRatioBasedSampler(otelSettings.GetSamplingRatio()))
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
            })
            .AddHttpClientInstrumentation(options => { options.RecordException = true; })
            .AddSource(otelSettings.ServiceName)
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(otelSettings.ExporterOtlpEndpoint);
            })
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(otelSettings.ServiceName)
            .AddMeter("TiHoMo.Gateway.Auth") // Add auth metrics
            .AddPrometheusExporter());

    // Add OpenAPI/Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    app.UseGlobalExceptionHandling();

    // ✅ Add CorrelationMiddleware for correlation ID tracking and structured logging
    // Thêm CorrelationMiddleware cho correlation ID tracking và structured logging
    app.UseMiddleware<CorrelationMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocelot Gateway API V1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.UseRequestLogging();

    // ✅ Add TracingMiddleware for distributed tracing
    // Thêm TracingMiddleware cho distributed tracing
    app.UseDistributedTracing();

    app.UseCors(corsSettings.PolicyName);

    // Add health route middleware BEFORE API Key middleware to intercept health requests
    app.UseMiddleware<HealthRouteMiddleware>();

    // Add simple API Key to JWT middleware BEFORE authentication (converts API Key to JWT Bearer token)
    app.UseMiddleware<ApiKeyToJwtMiddleware>();

    app.UseAuthentication();
    
    app.UseAuthorization();

    // Note: Gateway metrics are exposed via OpenTelemetry Prometheus exporter
    // No need for explicit MapMetrics() endpoint in Gateway
    
    // Use Ocelot for downstream routing (after local routes are handled)
    await app.UseOcelot();

    Log.Information("Ocelot Gateway started successfully on {BaseUrl}",
        builder.Configuration["GlobalConfiguration:BaseUrl"] ?? "http://localhost:5000");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Ocelot Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}