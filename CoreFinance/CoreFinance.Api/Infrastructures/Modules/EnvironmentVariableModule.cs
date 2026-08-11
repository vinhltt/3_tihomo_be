using Shared.Contracts.ConfigurationOptions;

namespace CoreFinance.Api.Infrastructures.Modules;

/// <summary>
///     Represents a module for accessing environment variables and configuring application settings. (EN)<br />
///     Đại diện cho một module để truy cập các biến môi trường và cấu hình cài đặt ứng dụng. (VI)
/// </summary>
public static class EnvironmentVariableModule
{
    /// <summary>
    ///     Adds configuration settings from appsettings.json and environment variables to the service collection. (EN)<br />
    ///     Thêm cài đặt cấu hình từ appsettings.json và các biến môi trường vào service collection. (VI)
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder.</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddConfigurationSettings(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        services.Configure<DbSettingOptions>(configuration.GetSection(nameof(DbSettingOptions)));
        services.Configure<CorsOptions>(configuration.GetSection("CorsOptions"));

        return services;
    }
}