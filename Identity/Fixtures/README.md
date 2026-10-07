# Identity baseline fixture (THM-2)

`identity-baseline-openiddict58.sql` = plain-SQL dump (`pg_restore --no-owner --no-privileges`) of the **net9 / OpenIddict 5.8 baseline** Identity database:

- schema at migration `20250706135843_EnhancedApiKeyManagement` (4 rows in `__EFMigrationsHistory`, `OpenIddictTokens.type` = varchar(50));
- synthetic rows written by the 5.8 managers: client `thm2-legacy-client` (`["ept:token","gt:refresh_token"]`) and two valid tokens, subject `thm2-legacy-subject`, types `access_token` / `refresh_token`;
- the rows the original migrations seed themselves (roles, `o_auth_clients`). No users, no credentials, no production data.

`run-backend-characterization.sh` always loads it into `thm2_identity` before Identity starts, so net10 Identity applies `20261005142618_UpgradeOpenIddictTokenType` on legacy data and `OpenIddictStoreParityTests.Legacy_Baseline_Rows_*` has the rows it asserts on.

## Regenerating (only if the baseline schema must change)

Needs a **net9 build of the pre-cutover source** (git history before THM-2 phase 03), not the current tree:

1. Create an empty PostgreSQL DB and start the net9 Identity API against it (it migrates to the baseline schema).
2. Seed with the 5.8 stores, from a throwaway net9 console referencing the net9 Identity publish output:

```csharp
var s = new ServiceCollection(); s.AddLogging();
s.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
s.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(args[0]).UseSnakeCaseNamingConvention());
s.AddOpenIddict().AddCore(o => o.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>());
await using var sp = s.BuildServiceProvider(); await using var scope = sp.CreateAsyncScope();
var apps = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
var tokens = scope.ServiceProvider.GetRequiredService<IOpenIddictTokenManager>();
var app = await apps.CreateAsync(new OpenIddictApplicationDescriptor { ClientId = "thm2-legacy-client", DisplayName = "Legacy",
  Permissions = { OpenIddictConstants.Permissions.Endpoints.Token, OpenIddictConstants.Permissions.GrantTypes.RefreshToken } });
foreach (var t in new[] { "access_token", "refresh_token" })
  await tokens.CreateAsync(new OpenIddictTokenDescriptor { ApplicationId = await apps.GetIdAsync(app), Subject = "thm2-legacy-subject",
    Type = t, Status = OpenIddictConstants.Statuses.Valid, CreationDate = DateTimeOffset.UtcNow, ExpirationDate = DateTimeOffset.UtcNow.AddDays(1) });
```

3. `pg_dump --format=custom`, then `pg_restore --no-owner --no-privileges -f identity-baseline-openiddict58.sql <dump>`.

Do not edit the SQL by hand: it must stay a real 5.8 artifact.
