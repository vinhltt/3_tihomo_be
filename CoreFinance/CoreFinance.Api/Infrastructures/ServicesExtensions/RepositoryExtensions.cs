using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Entities;
using CoreFinance.Infrastructure.Repositories.Base;
using Shared.Contracts.Extensions;

namespace CoreFinance.Api.Infrastructures.ServicesExtensions;

/// <summary>
///     Provides extension methods for registering repositories. (EN)<br />
///     Cung cấp các extension methods để đăng ký các repository. (VI)
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    ///     Registers base repositories with proxied scoped lifetime. (EN)<br />
    ///     Đăng ký các repository cơ sở với lifetime scoped được proxy. (VI)
    /// </summary>
    /// <param name="services">
    ///     The IServiceCollection to add repositories to. (EN)<br />
    ///     IServiceCollection để thêm các repository vào. (VI)
    /// </param>
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddProxiedScoped<IBaseRepository<Account, Guid>, BaseRepository<Account, Guid>>();
        services.AddProxiedScoped<IBaseRepository<Transaction, Guid>, BaseRepository<Transaction, Guid>>();
        services.AddProxiedScoped<IBaseRepository<ExpectedTransaction, Guid>, BaseRepository<ExpectedTransaction, Guid>>();
        services.AddProxiedScoped<IBaseRepository<RecurringTransactionTemplate, Guid>, BaseRepository<RecurringTransactionTemplate, Guid>>();
    }
}