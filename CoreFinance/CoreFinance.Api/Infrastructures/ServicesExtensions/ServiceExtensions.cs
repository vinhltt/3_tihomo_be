using Castle.DynamicProxy;
using CoreFinance.Api.Infrastructures.Interceptors;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services;
using CoreFinance.Domain.UnitOfWorks;
using CoreFinance.Infrastructure;
using CoreFinance.Infrastructure.UnitOfWorks;
using Shared.Contracts.Extensions;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for configuring application services and related dependencies. (EN)<br />
///     Cung cấp các extension methods để cấu hình các dịch vụ ứng dụng và các dependency liên quan. (VI)
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    ///     Registers application services, interceptors, and the Unit of Work. (EN)<br />
    ///     Đăng ký các dịch vụ ứng dụng, interceptors và Unit of Work. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection instance. (EN)<br />
    ///     Instance của IServiceCollection. (VI)
    /// </param>
    public static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IProxyGenerator, ProxyGenerator>();
        services.AddScoped<IAsyncInterceptor, MonitoringInterceptor>();

        services.AddScoped<IUnitOfWork, UnitOfWork<CoreFinanceDbContext>>();

        services.AddProxiedScoped<IAccountService, AccountService>();
        services.AddProxiedScoped<ITransactionService, TransactionService>();
        services.AddProxiedScoped<IRecurringTransactionTemplateService, RecurringTransactionTemplateService>();
        services.AddProxiedScoped<IExpectedTransactionService, ExpectedTransactionService>();
    }
}