using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Domain.Dtos.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _audience = configuration["Jwt:Audience"] ??
                                        throw new InvalidOperationException("JWT audience not configured");

    private readonly string _issuer =
        configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT issuer not configured");

    private readonly string _secretKey = configuration["Jwt:SecretKey"] ??
                                         throw new InvalidOperationException("JWT secret key not configured");

    public TimeSpan AccessTokenLifetime =>
        TimeSpan.FromMinutes(configuration.GetValue("Jwt:AccessTokenExpirationMinutes", 30));

    public TimeSpan RefreshTokenLifetime =>
        TimeSpan.FromDays(configuration.GetValue("Jwt:RefreshTokenExpirationDays", 7));

    public string GenerateAccessToken(UserProfile user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            // Standard claims
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("username", user.Username),
        };

        // Add roles as claims
        foreach (var role in user.Roles) claims.Add(new Claim(ClaimTypes.Role, role));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(AccessTokenLifetime),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<string?> ValidateTokenAsync(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = await tokenHandler.ValidateTokenAsync(token, validationParameters);

            if (principal.IsValid)
            {
                var userIdClaim = principal.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim?.Value;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}