using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.Extensions;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Identity.Infrastructure.Data;

public class IdentityDbContext : DbContext
{
    public const string DEFAULT_CONNECTION_STRING = "IdentityDb";

    // ReSharper disable once NotAccessedField.Local
    private readonly IConfiguration? _configuration;

    public IdentityDbContext()
    {
        _configuration = null;
    }
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options,
        IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ApiKeyUsageLog> ApiKeyUsageLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }

    // OpenIddict entities
    public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.GoogleId).IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.GoogleId).HasMaxLength(100);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.PictureUrl).HasMaxLength(500);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.EmailConfirmed).IsRequired().HasDefaultValue(false);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();

            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Permissions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ??
                         new List<string>());
        });

        // ApiKey configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.KeyPrefix);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.KeyHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.KeyPrefix).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Scopes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ??
                         new List<string>());

            // Enhanced properties
            entity.Property(e => e.IpWhitelist)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ??
                         new List<string>());

            entity.Property(e => e.RateLimitPerMinute).HasDefaultValue(100);
            entity.Property(e => e.DailyUsageQuota).HasDefaultValue(10000);
            entity.Property(e => e.TodayUsageCount).HasDefaultValue(0);
            entity.Property(e => e.UsageCount).HasDefaultValue(0);

            // Security Settings as JSON
            entity.Property(e => e.SecuritySettings)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<ApiKeySecuritySettings>(v, (JsonSerializerOptions?)null) ??
                         new ApiKeySecuritySettings());

            // Navigation properties
            entity.HasOne(e => e.User)
                .WithMany(u => u.ApiKeys)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.UsageLogs)
                .WithOne(ul => ul.ApiKey)
                .HasForeignKey(ul => ul.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApiKeyUsageLog configuration
        modelBuilder.Entity<ApiKeyUsageLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApiKeyId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Method);
            entity.HasIndex(e => e.StatusCode);
            entity.HasIndex(e => new { e.ApiKeyId, e.Timestamp });

            entity.Property(e => e.Method).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Endpoint).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45).IsRequired(); // IPv6 max length
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            entity.Property(e => e.RequestId).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.ResponseTime).HasDefaultValue(0);
            entity.Property(e => e.RequestSize).HasDefaultValue(0);
            entity.Property(e => e.ResponseSize).HasDefaultValue(0);

            // ScopesUsed as JSON
            entity.Property(e => e.ScopesUsed)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ??
                         new List<string>());

            entity.HasOne(e => e.ApiKey)
                .WithMany(ak => ak.UsageLogs)
                .HasForeignKey(e => e.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserRole many-to-many configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserLogin configuration
        modelBuilder.Entity<UserLogin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();

            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProviderUserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(200);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();

            entity.Property(e => e.Token).HasMaxLength(255).IsRequired();
            entity.Property(e => e.RevokedBy).HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OAuthClient configuration
        modelBuilder.Entity<OAuthClient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ClientId).IsUnique();

            entity.Property(e => e.ClientId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ClientSecretHash).HasMaxLength(255);
            entity.Property(e => e.RedirectUris).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.PostLogoutRedirectUris).HasMaxLength(2000);
            entity.Property(e => e.AllowedScopes).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ApplicationUrl).HasMaxLength(500);
        });

        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "User",
                Description = "Standard user role",
                Permissions = ["read:profile", "update:profile"],
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Admin",
                Description = "Administrator role",
                Permissions = ["*"],
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed default OAuth clients
        modelBuilder.Entity<OAuthClient>().HasData(
            new OAuthClient
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ClientId = "tihomo-web-client",
                Name = "TiHoMo Web Application",
                Description = "Nuxt.js web application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.Web,
                RedirectUris = "http://localhost:3500/auth/callback,https://app.tihomo.vn/auth/callback",
                PostLogoutRedirectUris = "http://localhost:3500,https://app.tihomo.vn",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days
                AllowRefreshTokens = true,
                RequirePkce = true,                ApplicationUrl = "https://app.tihomo.vn",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new OAuthClient
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                ClientId = "tihomo-mobile-ios",
                Name = "TiHoMo iOS Application",
                Description = "iOS mobile application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.IOs,
                RedirectUris = "tihomo://auth/callback,tihomo-ios://auth/callback",
                PostLogoutRedirectUris = "tihomo://logout,tihomo-ios://logout",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days                AllowRefreshTokens = true,
                RequirePkce = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new OAuthClient
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                ClientId = "tihomo-mobile-android",
                Name = "TiHoMo Android Application",
                Description = "Android mobile application client",
                Type = OAuthClientType.Public,
                Platform = ClientPlatform.Android,
                RedirectUris = "tihomo://auth/callback,tihomo-android://auth/callback",
                PostLogoutRedirectUris = "tihomo://logout,tihomo-android://logout",
                AllowedScopes = "openid,profile,email,offline_access",
                IsActive = true,
                AccessTokenLifetime = 3600, // 1 hour
                RefreshTokenLifetime = 2592000, // 30 days                AllowRefreshTokens = true,
                RequirePkce = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

        base.OnModelCreating(modelBuilder);
        modelBuilder.UseQueryFilter();

        // Configure OpenIddict entities
        modelBuilder.UseOpenIddict();
    }

    /// <summary>
    ///     (EN) Configures the database context.<br />
    ///     (VI) Cấu hình ngữ cảnh cơ sở dữ liệu.
    /// </summary>
    /// <param name="optionsBuilder">
    ///     The options builder. (EN)<br />
    ///     Bộ xây dựng tùy chọn. (VI)
    /// </param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        var connectionString = _configuration!.GetConnectionString(DEFAULT_CONNECTION_STRING);
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention(); // Add snake_case naming convention
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = DateTime.Now.ToString("yyyyMMddHHmmss");
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }
}