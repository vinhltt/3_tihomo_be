using CoreFinance.Domain.UnitOfWorks;
using CoreFinance.Infrastructure;
using CoreFinance.Infrastructure.UnitOfWorks;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for dependency injection and service configuration. (EN)<br />
///     Cung cấp các extension methods để tiêm dependency và cấu hình dịch vụ. (VI)
/// </summary>
public static class InjectionServiceExtension
{
    /// <summary>
    ///     Adds various services and dependencies to the service collection, including HttpContextAccessor, AutoMapper,
    ///     UnitOfWork, Validators, Repositories, and Application Services. (EN)<br />
    ///     Thêm các dịch vụ và dependency khác nhau vào service collection, bao gồm HttpContextAccessor, AutoMapper,
    ///     UnitOfWork, Validators, Repositories và Application Services. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection to add services to. (EN)<br />
    ///     IServiceCollection để thêm các dịch vụ vào. (VI)
    /// </param>
    public static void AddInjectedServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        // AutoMapper is registered in GeneralServiceExtension
        services.AddScoped<IUnitOfWork, UnitOfWork<CoreFinanceDbContext>>();
        services.AddApplicationValidators();
        services.AddRepositories();
        services.AddServices();
    }
}