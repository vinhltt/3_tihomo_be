using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Gateway.Configuration;
using Ocelot.Gateway.Middleware;

namespace Ocelot.Gateway.Extensions;

/// <summary>
///     Extension methods for configuring authentication services
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    ///     Add JWT Bearer authentication and ApiKey authentication
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="jwtSettings">JWT configuration settings</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
            {
                // Set JWT Bearer as default - it will be tried first
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // If no Authorization header or doesn't start with Bearer, let other schemes handle
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.NoResult();
                            return Task.CompletedTask;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogError(context.Exception, "JWT authentication failed");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("JWT token validated successfully for user: {User}",
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogDebug("JWT authentication challenge triggered: {Error}", context.Error);
                        
                        // Don't handle challenge if no Bearer token was present - let other schemes handle
                        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.HandleResponse();
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddScheme<ApiKeyAuthenticationSchemeOptions, DynamicApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationSchemeOptions.DefaultScheme,
                options => { });

        return services;
    }

    /// <summary>
    ///     Add authorization policies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("Admin"))
            .AddPolicy("UserPolicy", policy =>
                policy.RequireRole("User", "Admin"))
            .AddPolicy("ApiKeyPolicy", policy =>
                policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(ApiKeyAuthenticationSchemeOptions.DefaultScheme))
            .AddPolicy("FlexiblePolicy", policy =>
                policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme,
                        ApiKeyAuthenticationSchemeOptions.DefaultScheme));

        return services;
    }
}