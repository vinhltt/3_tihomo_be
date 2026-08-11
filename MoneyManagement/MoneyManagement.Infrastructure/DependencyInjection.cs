using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoneyManagement.Domain.BaseRepositories;
using MoneyManagement.Domain.UnitOfWorks;
using MoneyManagement.Infrastructure.Repositories.Base;
using MoneyManagement.Infrastructure.UnitOfWorks;

namespace MoneyManagement.Infrastructure;

/// <summary>
///     Dependency injection configuration for Infrastructure layer (EN)<br />
///     Cấu hình dependency injection cho lớp Infrastructure (VI)
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds infrastructure services to the service collection (EN)<br />
    ///     Thêm các dịch vụ infrastructure vào collection dịch vụ (VI)
    /// </summary>
    /// <param name="services">The service collection (EN)<br />Collection dịch vụ (VI)</param>
    /// <param name="configuration">The configuration (EN)<br />Cấu hình (VI)</param>
    /// <returns>The updated service collection</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database - Use InMemory for development if PostgreSQL is not available
        var connectionString = configuration.GetConnectionString("MoneyManagementDb");
        var useInMemory = bool.Parse(configuration["UseInMemoryDatabase"] ?? "false");        
        if (useInMemory)
            services.AddDbContext<MoneyManagementDbContext>(options =>
                options.UseInMemoryDatabase("MoneyManagementDb"));
        else
            services.AddDbContext<MoneyManagementDbContext>(options =>
                options.UseNpgsql(connectionString)
                       .UseSnakeCaseNamingConvention());

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork<MoneyManagementDbContext>>();

        // Base Repository
        services.AddScoped(typeof(IBaseRepository<,>), typeof(BaseRepository<,>));

        return services;
    }
}