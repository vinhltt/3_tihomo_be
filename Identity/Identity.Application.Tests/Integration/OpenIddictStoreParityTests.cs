using FluentAssertions;
using Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace Identity.Application.Tests.Integration;

/// <summary>
///     Characterization: OpenIddict application/token stores on a real PostgreSQL clone of baseline data (EN)<br/>
///     Characterization: store application/token của OpenIddict trên PostgreSQL clone dữ liệu baseline (VI)
/// </summary>
/// <remarks>
///     Requires TIHOMO_TEST_IDENTITY_DB (migrated Identity schema). Legacy rows seeded by the 5.8 baseline are read back,
///     updated and revoked through the current managers; nothing here is a forwarding/mock test.
/// </remarks>
[Trait("Category", "Characterization")]
public class OpenIddictStoreParityTests
{
    private const string LegacyClientId = "thm2-legacy-client";

    private static ServiceProvider Services()
    {
        var cs = Environment.GetEnvironmentVariable("TIHOMO_TEST_IDENTITY_DB") is { Length: > 0 } v
            ? v
            : throw new InvalidOperationException("TIHOMO_TEST_IDENTITY_DB is not set; this characterization must not be skipped.");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(cs).UseSnakeCaseNamingConvention());
        services.AddOpenIddict().AddCore(o => o.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>());
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Stores_Should_Roundtrip_Applications_And_Tokens_Including_Long_Uri_Types_And_Revocation()
    {
        await using var sp = Services();
        await using var scope = sp.CreateAsyncScope();
        var apps = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();

        var clientId = $"thm2-client-{Guid.CreateVersion7():N}";
        var app = await apps.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "Characterization client",
            RedirectUris = { new Uri("https://localhost/callback") },
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization, OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, OpenIddictConstants.Permissions.Scopes.Email
            }
        });

        var reloaded = await apps.FindByClientIdAsync(clientId);
        reloaded.Should().NotBeNull();
        (await apps.GetPermissionsAsync(reloaded!)).Should().BeEquivalentTo([
            OpenIddictConstants.Permissions.Endpoints.Authorization, OpenIddictConstants.Permissions.Endpoints.Token,
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode, OpenIddictConstants.Permissions.Scopes.Email
        ]);
        (await apps.GetRedirectUrisAsync(reloaded!)).Should().ContainSingle().Which.Should().Be("https://localhost/callback");

        // A URI-style token type longer than the old 50-char column: requires the width migration.
        const string longType = "urn:ietf:params:oauth:token-type:refresh_token+characterization";
        longType.Length.Should().BeGreaterThan(50);
        var token = await tokens.CreateAsync(new OpenIddictTokenDescriptor
        {
            ApplicationId = await apps.GetIdAsync(app), Subject = "thm2-subject", Type = longType,
            Status = OpenIddictConstants.Statuses.Valid, CreationDate = DateTimeOffset.UtcNow,
            ExpirationDate = DateTimeOffset.UtcNow.AddHours(1)
        });
        var tokenId = await tokens.GetIdAsync(token);
        (await tokens.GetTypeAsync((await tokens.FindByIdAsync(tokenId!))!)).Should().Be(longType);

        (await tokens.TryRevokeAsync(token)).Should().BeTrue();
        var revoked = await tokens.FindByIdAsync(tokenId!);
        (await tokens.HasStatusAsync(revoked!, OpenIddictConstants.Statuses.Revoked)).Should().BeTrue();
    }

    [Fact]
    public async Task Legacy_Baseline_Rows_Should_Remain_Readable_Updatable_And_Revocable()
    {
        await using var sp = Services();
        await using var scope = sp.CreateAsyncScope();
        var apps = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var legacy = await apps.FindByClientIdAsync(LegacyClientId);
        legacy.Should().NotBeNull("the clone must contain the client seeded by the OpenIddict 5.8 baseline");
        (await apps.GetPermissionsAsync(legacy!)).Should().Contain(OpenIddictConstants.Permissions.Endpoints.Token);

        var legacyTokens = await db.Set<OpenIddictEntityFrameworkCoreToken>().AsNoTracking()
            .Where(t => t.Subject == "thm2-legacy-subject").OrderBy(t => t.Type).ToListAsync();
        legacyTokens.Select(t => t.Type).Should().BeEquivalentTo(["access_token", "refresh_token"],
            "legacy short type strings are kept as-is, not rewritten");
        legacyTokens.Should().OnlyContain(t => t.Status == OpenIddictConstants.Statuses.Valid);

        var refresh = await tokens.FindByIdAsync(legacyTokens.Single(t => t.Type == "refresh_token").Id!);
        (await tokens.TryRevokeAsync(refresh!)).Should().BeTrue();
        (await tokens.HasStatusAsync((await tokens.FindByIdAsync(legacyTokens.Single(t => t.Type == "refresh_token").Id!))!,
            OpenIddictConstants.Statuses.Revoked)).Should().BeTrue();

        var revokedCount = await tokens.RevokeBySubjectAsync("thm2-legacy-subject");
        revokedCount.Should().Be(1, "bulk revocation still works on EF Core 10 and only touches the remaining valid token");
    }
}
