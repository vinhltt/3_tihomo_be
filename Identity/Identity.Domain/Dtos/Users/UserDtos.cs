using Identity.Domain.Dtos.Common;
using Identity.Domain.Dtos.Roles;

namespace Identity.Domain.Dtos.Users;

public record UserDetailResponse(
    Guid Id,
    string Email,
    string Username,
    string FullName,
    string? AvatarUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    List<RoleResponse> Roles);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string Username,
    string FullName,
    string? AvatarUrl,
    bool IsActive,
    DateTime CreatedAt);

public record UpdateUserRequest(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    string? Username = null,
    string? PhoneNumber = null);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);

public record CreateUserRequest(
    string Email,
    string Username,
    string FullName,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Password);

public record UserResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public string? GoogleId { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record UserFilterRequest : BaseFilterRequest
{
    public string? Email { get; init; }
    public string? FullName { get; init; }
    public bool? IsActive { get; init; }
}

public record UserListItem(
    Guid Id,
    string Email,
    string Username,
    string FullName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);