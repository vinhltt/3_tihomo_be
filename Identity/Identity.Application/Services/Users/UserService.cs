using Identity.Application.Common.Interfaces;
using Identity.Domain.Dtos.Common;
using Identity.Domain.Dtos.Roles;
using Identity.Domain.Dtos.Users;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;

namespace Identity.Application.Services.Users;

/// <summary>
///     Service for managing user operations (EN)<br />
///     Dịch vụ quản lý các thao tác người dùng (VI)
/// </summary>
/// <param name="userRepository">
///     Repository for user data access (EN)<br />
///     Repository để truy cập dữ liệu người dùng (VI)
/// </param>
/// <param name="passwordHasher">
///     Service for password hashing and verification (EN)<br />
///     Dịch vụ để băm và xác minh mật khẩu (VI)
/// </param>
/// <param name="roleRepository">
///     Repository for role data access (EN)<br />
///     Repository để truy cập dữ liệu vai trò (VI)
/// </param>
public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IRoleRepository roleRepository)
    : IUserService
{
    public async Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user == null ? null : MapToUserResponse(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        return user == null ? null : MapToUserResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

        // Validate email uniqueness if changed
        if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
        {
            if (await userRepository.IsEmailExistsAsync(request.Email, userId, cancellationToken))
                throw new InvalidOperationException("Email is already taken");
            user.Email = request.Email;
        }

        // Validate username uniqueness if changed
        if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
        {
            if (await userRepository.IsUsernameExistsAsync(request.Username, userId, cancellationToken))
                throw new InvalidOperationException("Username is already taken");
            user.Username = request.Username;
        } // Update other fields - combine FirstName and LastName into FullName

        if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
        {
            var firstName = !string.IsNullOrEmpty(request.FirstName) ? request.FirstName : "";
            var lastName = !string.IsNullOrEmpty(request.LastName) ? request.LastName : "";
            user.FullName = $"{firstName} {lastName}".Trim();
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user, cancellationToken);

        return MapToUserResponse(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

        // For Google users who don't have a password yet, skip current password verification
        if (!string.IsNullOrEmpty(user.PasswordHash) &&
            !passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        // Validate new password confirmation
        if (request.NewPassword != request.ConfirmPassword)
            throw new ArgumentException("New password and confirmation do not match");

        // Hash new password
        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(user, cancellationToken);
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check if email is already taken
        if (await userRepository.IsEmailExistsAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Email is already taken");

        // Check if username is already taken
        if (await userRepository.IsUsernameExistsAsync(request.Username, cancellationToken))
            throw new InvalidOperationException("Username is already taken");
        var user = new User
        {
            Id = Guid.CreateVersion7(),
            Email = request.Email,
            Username = request.Username,
            FullName = request.FullName,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            IsActive = true,
            EmailConfirmed = false, // New users need to confirm their email
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        return MapToUserResponse(user);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException($"User with ID {userId} not found");

        await userRepository.DeleteAsync(userId, cancellationToken);
    }

    public async Task<PagedResponse<UserResponse>> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetPagedAsync(page, pageSize, search, cancellationToken);
        var totalCount = await userRepository.GetTotalCountAsync(search, cancellationToken);

        var userResponses = users.Select(MapToUserResponse).ToList();

        return new PagedResponse<UserResponse>
        {
            Items = userResponses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }

    public async Task<bool> ValidatePasswordAsync(Guid userId, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash)) return false;

        return passwordHasher.VerifyPassword(password, user.PasswordHash);
    }

    public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (excludeUserId.HasValue)
            return await userRepository.IsEmailExistsAsync(email, excludeUserId.Value, cancellationToken);

        return await userRepository.IsEmailExistsAsync(email, cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (excludeUserId.HasValue)
            return await userRepository.IsUsernameExistsAsync(username, excludeUserId.Value, cancellationToken);

        return await userRepository.IsUsernameExistsAsync(username, cancellationToken);
    }

    public async Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetUserRolesAsync(userId, cancellationToken);
        return roles.Select(MapToRoleResponse);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            GoogleId = user.GoogleId,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    private static RoleResponse MapToRoleResponse(Role role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = role.Permissions,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }
}