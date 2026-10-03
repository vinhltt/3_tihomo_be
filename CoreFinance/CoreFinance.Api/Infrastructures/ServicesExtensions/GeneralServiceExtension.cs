using System.Text.Json;
using System.Text.Json.Serialization;
using CoreFinance.Api.Infrastructures.Swagger.SchemaFilters;
using CoreFinance.Application.Mapper;
using CoreFinance.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Contracts.ConfigurationOptions;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for configuring general services and middleware in the API. (EN)<br />
///     Cung cấp các extension methods để cấu hình các dịch vụ và middleware chung trong API. (VI)
/// </summary>
public static class GeneralServiceExtension
{
    /// <summary>
    ///     Adds general service configurations, including DbContext, OpenAPI, AutoMapper, JSON options, SwaggerGen, and CORS.
    ///     (EN)<br />
    ///     Bổ sung các cấu hình dịch vụ chung, bao gồm DbContext, OpenAPI, AutoMapper, tùy chọn JSON, SwaggerGen và CORS. (VI)
    /// </summary>
    /// <param name="builder">
    ///     The WebApplicationBuilder instance. (EN)<br />
    ///     Instance của WebApplicationBuilder. (VI)
    /// </param>
    /// <param name="policyName">
    ///     The name of the CORS policy to add. (EN)<br />
    ///     Tên của chính sách CORS cần thêm. (VI)
    /// </param>
    /// <param name="corsOption">
    ///     The CORS options configuration. (EN)<br />
    ///     Cấu hình tùy chọn CORS. (VI)
    /// </param>
    public static void AddGeneralConfigurations(
        this WebApplicationBuilder builder,
        string policyName,
        CorsOptions corsOption
    )
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(corsOption);

        var dbSettingOptions = builder.Configuration.GetOptions<DbSettingOptions>(nameof(DbSettingOptions));

        // Skip PostgreSQL DbContext registration in Testing environment (tests will register InMemory)
        if (builder.Environment.EnvironmentName != "Testing")
        {
            builder.Services.AddDbContext<CoreFinanceDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString(CoreFinanceDbContext.DEFAULT_CONNECTION_STRING),
                    _ => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true));

                options.UseSnakeCaseNamingConvention();
                options.EnableDetailedErrors(dbSettingOptions?.EnableDetailedErrors ?? false);
                options.EnableSensitiveDataLogging(dbSettingOptions?.EnableSensitiveDataLogging ?? false);
            });
        }
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // FluentValidation

        // AutoMapper
        builder.Services.AddAutoMapper(_ => { }, typeof(AutoMapperProfile).Assembly);

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                {
                    // Ví dụ: sử dụng camelCase cho tên thuộc tính
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

                    // Ví dụ: bỏ qua các thuộc tính có giá trị null khi serialize
                    options.JsonSerializerOptions.DefaultIgnoreCondition =
                        JsonIgnoreCondition.WhenWritingNull;

                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                }
            );
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "App Api", Version = "v1" });

            // Include XML comments for better API documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Include Application layer XML comments for DTOs
            var appXmlFile = "CoreFinance.Application.xml";
            var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
            if (File.Exists(appXmlPath))
            {
                c.IncludeXmlComments(appXmlPath);
            }

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Example Token: 'Bearer {Token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oath2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
            c.SchemaFilter<EnumSchemaFilter>();
        });

        builder.Services.AddCors(c =>
        {
            c.AddPolicy(policyName, options =>
            {
                options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                if (corsOption.AllowedOrigins.IsAllowedAll())
                    options.AllowAnyOrigin();
                else
                    options.WithOrigins(corsOption.AllowedOrigins);

                if (corsOption.AllowedMethods.IsAllowedAll())
                    options.AllowAnyMethod();
                else
                    options.WithMethods(corsOption.AllowedMethods);

                if (corsOption.ExposedHeaders.IsAllowedAll())
                    options.AllowAnyHeader();
                else
                    options.WithHeaders(corsOption.ExposedHeaders);
            });
        });
    }

    /// <summary>
    ///     Checks if the provided collection of strings indicates that all values are allowed (contains "*"). (EN)<br />
    ///     Kiểm tra xem tập hợp chuỗi được cung cấp có cho biết tất cả các giá trị đều được phép hay không (chứa "*"). (VI)
    /// </summary>
    /// <param name="values">
    ///     The collection of strings to check. (EN)<br />
    ///     Tập hợp chuỗi cần kiểm tra. (VI)
    /// </param>
    /// <returns>
    ///     True if the collection is null, empty, or contains "*"; otherwise, false. (EN)<br />
    ///     True nếu tập hợp là null, rỗng hoặc chứa "*"; ngược lại là false. (VI)
    /// </returns>
    private static bool IsAllowedAll(this IReadOnlyCollection<string>? values)
    {
        return values == null || values.Count == 0 || values.Contains("*");
    }
}