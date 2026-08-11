using System.Diagnostics;
using System.Text.Json;
using Identity.Infrastructure.Data;
using Identity.Domain.Entities;
using Identity.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Identity.Api.Services;

/// <summary>
///     Enhanced user service with multi-level caching and optimized database operations
///     Service người dùng nâng cao với caching đa tầng và tối ưu hóa operations database
/// </summary>
public class EnhancedUserService(
    IdentityDbContext context,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    ILogger<EnhancedUserService> logger)
    : IUserService
{
    /// <summary>
    ///     Get user by ID with multi-level caching
    ///     Lấy user theo ID với caching đa tầng
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // L1 Cache (Memory) - 5 minute TTL for active users
            var userCacheKey = $"user_id:{userId}";
            if (memoryCache.TryGetValue(userCacheKey, out User? cachedUser))
            {
                logger.LogDebug("User retrieved from L1 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return cachedUser;
            }

            // L2 Cache (Redis) - 15 minute TTL
            var distributedUser = await GetUserFromDistributedCacheAsync(userCacheKey);
            if (distributedUser != null)
            {
                memoryCache.Set(userCacheKey, distributedUser, TimeSpan.FromMinutes(5));
                logger.LogDebug("User retrieved from L2 cache in {Duration}ms", stopwatch.ElapsedMilliseconds);
                return distributedUser;
            }

            // Database fallback
            var dbUser = await context.Users
                .Include(u => u.UserLogins)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (dbUser != null)
            {
                await CacheUserAsync(userCacheKey, dbUser);
                logger.LogDebug("User retrieved from database in {Duration}ms", stopwatch.ElapsedMilliseconds);
            }

            return dbUser;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user by ID {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    ///     Get user by email with caching
    ///     Lấy user theo email với caching
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            // Check cache first using email as key
            var emailCacheKey = $"user_email:{email.ToLowerInvariant()}";
            if (memoryCache.TryGetValue(emailCacheKey, out User? cachedUser)) return cachedUser;

            var user = await context.Users
                .Include(u => u.UserLogins)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // Cache by both ID and email for faster lookup
                await CacheUserAsync($"user_id:{user.Id}", user);
                memoryCache.Set(emailCacheKey, user, TimeSpan.FromMinutes(5));
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get user by email {Email}", email);
            return null;
        }
    }

    /// <summary>
    ///     Get or create user with optimized upsert pattern and caching
    ///     Lấy hoặc tạo user với upsert pattern tối ưu và caching
    /// </summary>
    public async Task<User?> GetOrCreateUserAsync(SocialUserInfo socialUserInfo)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Check cache first for faster lookup
            var providerCacheKey = $"user_provider:{socialUserInfo.Provider}:{socialUserInfo.Id}";
            if (memoryCache.TryGetValue(providerCacheKey, out User? cachedUser))
            {
                logger.LogDebug("User found in cache for provider {Provider} in {Duration}ms",
                    socialUserInfo.Provider, stopwatch.ElapsedMilliseconds);
                return cachedUser;
            }

            // Use atomic upsert pattern to handle concurrent requests
            var user = await UpsertUserAtomicAsync(socialUserInfo);

            if (user != null)
            {
                // Cache the user result for future requests
                await CacheUserMultipleKeysAsync(user, socialUserInfo.Provider, socialUserInfo.Id);
                logger.LogInformation("User upserted successfully for {Email} in {Duration}ms",
                    socialUserInfo.Email, stopwatch.ElapsedMilliseconds);
            }

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get or create user for {Email}", socialUserInfo.Email);
            return null;
        }
    }

    /// <summary>
    ///     Map user to UserInfo DTO
    ///     Map user thành UserInfo DTO
    /// </summary>
    public Task<UserInfo> MapToUserInfoAsync(User user)
    {
        var providers = user.UserLogins.Select(ul => ul.Provider).Distinct().ToList();

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Username = user.Username,
            PictureUrl = user.PictureUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
            Providers = providers
        };

        return Task.FromResult(userInfo);
    }

    /// <summary>
    ///     Update user with cache invalidation
    ///     Cập nhật user với cache invalidation
    /// </summary>
    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Invalidate all cache entries for this user
            await InvalidateUserCacheAsync(user);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user {UserId}", user.Id);
            return false;
        }
    }

    /// <summary>
    ///     Deactivate user with cache invalidation
    ///     Vô hiệu hóa user với cache invalidation
    /// </summary>
    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Invalidate cache entries
            await InvalidateUserCacheAsync(user);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deactivate user {UserId}", userId);
            return false;
        }
    }

    #region Private Helper Methods

    /// <summary>
    ///     Atomic upsert operation to prevent race conditions
    ///     Thao tác upsert atomic để tránh race conditions
    /// </summary>
    private async Task<User?> UpsertUserAtomicAsync(SocialUserInfo socialUserInfo)
    {
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Try to find existing user by email first
            var existingUser = await context.Users
                .Include(u => u.UserLogins)
                .FirstOrDefaultAsync(u => u.Email == socialUserInfo.Email);

            if (existingUser != null)
                // Update existing user
                return await UpdateExistingUserAsync(existingUser, socialUserInfo);

            // Create new user
            var newUser = await CreateNewUserAsync(socialUserInfo);

            await transaction.CommitAsync();
            return newUser;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    ///     Update existing user with new social login info
    ///     Cập nhật user hiện có với thông tin social login mới
    /// </summary>
    private async Task<User> UpdateExistingUserAsync(User existingUser, SocialUserInfo socialUserInfo)
    {
        // Check if this social login already exists
        var existingLogin = existingUser.UserLogins
            .FirstOrDefault(ul => ul.Provider == socialUserInfo.Provider &&
                                  ul.ProviderUserId == socialUserInfo.Id);

        if (existingLogin == null)
        {
            // Add new social login to existing user
            var newLogin = new UserLogin
            {
                UserId = existingUser.Id,
                Provider = socialUserInfo.Provider,
                ProviderUserId = socialUserInfo.Id,
                ProviderDisplayName = socialUserInfo.Name,
                LastLoginAt = DateTime.UtcNow
            };

            context.UserLogins.Add(newLogin);
        }
        else
        {
            // Update last login time
            existingLogin.LastLoginAt = DateTime.UtcNow;
        }

        // Update user info if needed (only if changed to avoid unnecessary updates)
        var hasChanges = false;
        if (existingUser.Name != socialUserInfo.Name)
        {
            existingUser.Name = socialUserInfo.Name;
            hasChanges = true;
        }

        if (existingUser.PictureUrl != socialUserInfo.PictureUrl)
        {
            existingUser.PictureUrl = socialUserInfo.PictureUrl;
            hasChanges = true;
        }

        if (hasChanges) existingUser.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existingUser;
    }

    /// <summary>
    ///     Create new user with social login
    ///     Tạo user mới với social login
    /// </summary>
    private async Task<User> CreateNewUserAsync(SocialUserInfo socialUserInfo)
    {
        var newUser = new User
        {
            Email = socialUserInfo.Email,
            Name = socialUserInfo.Name,
            Username = socialUserInfo.Email, // Use email as username
            PictureUrl = socialUserInfo.PictureUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(newUser);
        await context.SaveChangesAsync();

        // Add social login
        var userLogin = new UserLogin
        {
            UserId = newUser.Id,
            Provider = socialUserInfo.Provider,
            ProviderUserId = socialUserInfo.Id,
            ProviderDisplayName = socialUserInfo.Name,
            LastLoginAt = DateTime.UtcNow
        };

        context.UserLogins.Add(userLogin);
        await context.SaveChangesAsync();

        // Reload with navigation properties
        return await context.Users
            .Include(u => u.UserLogins)
            .FirstAsync(u => u.Id == newUser.Id);
    }

    /// <summary>
    ///     Get user from distributed cache
    ///     Lấy user từ distributed cache
    /// </summary>
    private async Task<User?> GetUserFromDistributedCacheAsync(string key)
    {
        try
        {
            var cachedData = await distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedData)) return JsonSerializer.Deserialize<User>(cachedData);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get user from distributed cache");
        }

        return null;
    }

    /// <summary>
    ///     Cache user in both memory and distributed cache
    ///     Cache user ở cả memory và distributed cache
    /// </summary>
    private async Task CacheUserAsync(string cacheKey, User user)
    {
        try
        {
            // L1 Cache (Memory) - 5 minute TTL
            memoryCache.Set(cacheKey, user, TimeSpan.FromMinutes(5));

            // L2 Cache (Redis) - 15 minute TTL
            var jsonData = JsonSerializer.Serialize(user);
            await distributedCache.SetStringAsync(cacheKey, jsonData,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to cache user");
        }
    }

    /// <summary>
    ///     Cache user with multiple cache keys for different lookup patterns
    ///     Cache user với nhiều cache keys cho các pattern lookup khác nhau
    /// </summary>
    private async Task CacheUserMultipleKeysAsync(User user, string provider, string providerId)
    {
        await CacheUserAsync($"user_id:{user.Id}", user);
        await CacheUserAsync($"user_provider:{provider}:{providerId}", user);
        memoryCache.Set($"user_email:{user.Email.ToLowerInvariant()}", user, TimeSpan.FromMinutes(5));
    }

    /// <summary>
    ///     Invalidate all cache entries for a user
    ///     Vô hiệu hóa tất cả cache entries cho một user
    /// </summary>
    private async Task InvalidateUserCacheAsync(User user)
    {
        try
        {
            // Remove from memory cache
            memoryCache.Remove($"user_id:{user.Id}");
            memoryCache.Remove($"user_email:{user.Email.ToLowerInvariant()}");

            // Remove provider-specific cache entries
            foreach (var login in user.UserLogins)
            {
                memoryCache.Remove($"user_provider:{login.Provider}:{login.ProviderUserId}");
                await distributedCache.RemoveAsync($"user_provider:{login.Provider}:{login.ProviderUserId}");
            }

            // Remove from distributed cache
            await distributedCache.RemoveAsync($"user_id:{user.Id}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to invalidate user cache");
        }
    }

    #endregion
}