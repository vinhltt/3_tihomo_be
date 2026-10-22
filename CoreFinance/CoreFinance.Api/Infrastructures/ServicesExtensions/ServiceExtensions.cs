using Castle.DynamicProxy;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using CoreFinance.Api.Constants;
using CoreFinance.Api.Infrastructures.Interceptors;
using CoreFinance.Application.Interfaces;
using CoreFinance.Application.Services;
using CoreFinance.Application.Utilities;
using CoreFinance.Domain.Constants;
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
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CoreFinanceAuthorizationPolicies.Owner, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    // Match CurrentUserContext: never accept an owner claim from an unauthenticated identity.
                    var claim = context.User.Identities
                        .Where(identity => identity.IsAuthenticated)
                        .SelectMany(identity => identity.FindAll(ClaimTypes.NameIdentifier))
                        .FirstOrDefault();
                    return Guid.TryParse(claim?.Value, out var ownerId) && ownerId != Guid.Empty;
                });
            });
        });

        // Authorization runs before controller activation. Shape errors only for this owner policy,
        // preserving the bearer scheme's existing events and all other controllers' responses.
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            var challenge = options.Events.OnChallenge;
            options.Events.OnChallenge = async context =>
            {
                await challenge(context);
                if (context.Handled || context.Response.HasStarted || !IsOwnerEndpoint(context.HttpContext)) return;
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate = "Bearer";
                await context.Response.WriteAsJsonAsync(
                    new { code = CoreFinanceErrorCodes.Unauthorized, message = CoreFinanceMessages.AuthenticationRequired },
                    context.HttpContext.RequestAborted);
            };
            var forbidden = options.Events.OnForbidden;
            options.Events.OnForbidden = async context =>
            {
                await forbidden(context);
                if (context.Response.HasStarted || !IsOwnerEndpoint(context.HttpContext)) return;
                await context.Response.WriteAsJsonAsync(
                    new { code = CoreFinanceErrorCodes.Forbidden, message = CoreFinanceMessages.ValidAuthenticatedOwnerClaimRequired },
                    context.HttpContext.RequestAborted);
            };
        });

        services.AddSingleton<IProxyGenerator, ProxyGenerator>();
        services.AddScoped<IAsyncInterceptor, MonitoringInterceptor>();

        services.AddScoped<IUnitOfWork, UnitOfWork<CoreFinanceDbContext>>();

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<BusinessCalendar>();

        services.AddProxiedScoped<IAccountService, AccountService>();
        services.AddProxiedScoped<ITransactionService, TransactionService>();
        services.AddProxiedScoped<IRecurringTransactionTemplateService, RecurringTransactionTemplateService>();
        services.AddProxiedScoped<IExpectedTransactionService, ExpectedTransactionService>();
    }

    private static bool IsOwnerEndpoint(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetOrderedMetadata<IAuthorizeData>()
            .Any(data => data.Policy == CoreFinanceAuthorizationPolicies.Owner) == true;
}