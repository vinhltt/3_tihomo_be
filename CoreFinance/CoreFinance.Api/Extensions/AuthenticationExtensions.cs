using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using CoreFinance.Api.Configuration;

namespace CoreFinance.Api.Extensions;

/// <summary>
///     Extension methods for configuring authentication services (EN)<br/>
///     Phương thức mở rộng để cấu hình dịch vụ xác thực (VI)
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    ///     Add JWT Bearer authentication (EN)<br/>
    ///     Thêm xác thực JWT Bearer (VI)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="jwtSettings">JWT configuration settings</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
    {
        var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
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
                    ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew tolerance for JWT validation
                };

                // Custom token retrieval from multiple sources
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        
                        // Debug: Log all request headers
                        var allHeaders = string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}:{h.Value}").ToArray());
                        logger.LogInformation("[DEBUG JWT] All request headers: {Headers}", allHeaders);
                        
                        // Try multiple sources for JWT token
                        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "")
                                   ?? context.Request.Headers["X-Forwarded-Auth"].FirstOrDefault()?.Replace("Bearer ", "")
                                   ?? context.Request.Headers["X-JWT-Token"].FirstOrDefault()
                                   ?? context.Request.Headers["X-TiHoMo-JWT"].FirstOrDefault()
                                   ?? context.Request.Headers["X-Custom-Auth"].FirstOrDefault()?.Replace("Bearer ", "")
                                   ?? context.HttpContext.Items["JWT_TOKEN"] as string
                                   ?? context.HttpContext.Items["FINAL_JWT_TOKEN"] as string;
                        
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                            logger.LogInformation("[DEBUG JWT] JWT token found from custom sources, length: {Length}", token.Length);
                        }
                        else
                        {
                            logger.LogWarning("[DEBUG JWT] No JWT token found in any source");
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
                        logger.LogInformation("JWT token validated successfully for user: {User}",
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                        logger.LogInformation("JWT authentication challenge triggered: {Error}", context.Error);
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    ///     Add authorization (EN)<br/>
    ///     Thêm phân quyền (VI)
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddJwtAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization();
        return services;
    }
}