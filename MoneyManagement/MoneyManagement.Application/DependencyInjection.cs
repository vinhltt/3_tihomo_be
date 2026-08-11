using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Application.Services;

namespace MoneyManagement.Application;

/// <summary>
///     Dependency injection configuration for Application layer (EN)<br />
///     Cấu hình dependency injection cho lớp Application (VI)
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds application services to the service collection (EN)<br />
    ///     Thêm các dịch vụ application vào collection dịch vụ (VI)
    /// </summary>
    /// <param name="services">The service collection (EN)<br />Collection dịch vụ (VI)</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IJarService, JarService>();
        services.AddScoped<ISharedExpenseService, SharedExpenseService>();

        // AutoMapper configuration
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // FluentValidation configuration
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}