using Identity.Infrastructure.Data;
using Identity.Domain.Entities;
using Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Services;

public interface IUserService
{
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetOrCreateUserAsync(SocialUserInfo socialUserInfo);
    Task<UserInfo> MapToUserInfoAsync(User user);
    Task<bool> UpdateUserAsync(User user);
    Task<bool> DeactivateUserAsync(Guid userId);
}

public class UserService(IdentityDbContext context, ILogger<UserService> logger) : IUserService
{
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users
            .Include(u => u.UserLogins)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await context.Users
            .Include(u => u.UserLogins)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetOrCreateUserAsync(SocialUserInfo socialUserInfo)
    {
        try
        {
            logger.LogInformation("GetOrCreateUserAsync started for email: {Email}, Provider: {Provider}", 
                socialUserInfo.Email, socialUserInfo.Provider);

            // First, try to find existing user by email
            logger.LogDebug("Searching for existing user with email: {Email}", socialUserInfo.Email);
            var existingUser = await GetUserByEmailAsync(socialUserInfo.Email);

            if (existingUser != null)
            {
                logger.LogInformation("Found existing user: Id={UserId}, Email={Email}", 
                    existingUser.Id, existingUser.Email);

                // Check if this social login already exists
                var existingLogin = existingUser.UserLogins
                    .FirstOrDefault(ul => ul.Provider == socialUserInfo.Provider &&
                                          ul.ProviderUserId == socialUserInfo.Id);

                if (existingLogin == null)
                {
                    logger.LogInformation("Adding new social login for provider {Provider} to user {UserId}", 
                        socialUserInfo.Provider, existingUser.Id);
                    
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
                    logger.LogDebug("Updating last login time for existing social login");
                    // Update last login time
                    existingLogin.LastLoginAt = DateTime.UtcNow;
                }

                // Update user info if needed
                if (existingUser.Name != socialUserInfo.Name || existingUser.PictureUrl != socialUserInfo.PictureUrl)
                {
                    logger.LogDebug("Updating user info: Name or Picture changed");
                    existingUser.Name = socialUserInfo.Name;
                    existingUser.PictureUrl = socialUserInfo.PictureUrl;
                    existingUser.UpdatedAt = DateTime.UtcNow;
                }

                logger.LogInformation("Saving changes for existing user {UserId}", existingUser.Id);
                await context.SaveChangesAsync();
                return existingUser;
            }

            logger.LogInformation("User not found, creating new user for email: {Email}", socialUserInfo.Email);

            // Create new user
            var newUser = new User
            {
                Email = socialUserInfo.Email,
                Name = socialUserInfo.Name,
                Username = socialUserInfo.Email, // Use email as username
                PictureUrl = socialUserInfo.PictureUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            logger.LogDebug("Adding new user to database");
            context.Users.Add(newUser);
            
            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("New user created successfully: Id={UserId}, Email={Email}", 
                    newUser.Id, newUser.Email);
            }
            catch (DbUpdateException dbEx)
            {
                logger.LogError(dbEx, "Database error when creating user. Possible duplicate or constraint violation");
                throw;
            }

            // Add social login
            logger.LogDebug("Adding social login for new user {UserId}", newUser.Id);
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
            logger.LogInformation("Social login added for user {UserId}", newUser.Id);

            // Reload with navigation properties
            logger.LogDebug("Reloading user with navigation properties");
            return await GetUserByIdAsync(newUser.Id);
        }
        catch (DbUpdateException dbEx)
        {
            logger.LogError(dbEx, "Database update error for {Email}. Connection string: {ConnString}", 
                socialUserInfo.Email, 
                context.Database.GetConnectionString()?.Contains("Host=") == true ? "Connected" : "Not Connected");
            
            if (dbEx.InnerException != null)
            {
                logger.LogError("Inner exception: {InnerMessage}", dbEx.InnerException.Message);
            }
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in GetOrCreateUserAsync for {Email}", socialUserInfo.Email);
            logger.LogError("Exception Type: {Type}, Message: {Message}", 
                ex.GetType().Name, ex.Message);
            
            if (ex.InnerException != null)
            {
                logger.LogError("Inner exception: Type={InnerType}, Message={InnerMessage}", 
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }
            
            // Log database connection status
            try
            {
                var canConnect = await context.Database.CanConnectAsync();
                logger.LogError("Database connection status: {Status}", canConnect ? "Connected" : "Cannot connect");
                
                if (!canConnect)
                {
                    var connString = context.Database.GetConnectionString();
                    logger.LogError("Connection string check: Host={HasHost}, Database={HasDb}", 
                        connString?.Contains("Host=") == true,
                        connString?.Contains("Database=") == true);
                }
            }
            catch (Exception connEx)
            {
                logger.LogError(connEx, "Failed to check database connection");
            }
            
            return null;
        }
    }

    public Task<UserInfo> MapToUserInfoAsync(User user)
    {
        var providers = user.UserLogins.Select(ul => ul.Provider).Distinct().ToList();

        var userInfo = new UserInfo
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            PictureUrl = user.PictureUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
            Providers = providers
        };

        return Task.FromResult(userInfo);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user {UserId}", user.Id);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deactivate user {UserId}", userId);
            return false;
        }
    }
}