using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

public class User : BaseEntity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PictureUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool EmailConfirmed { get; set; } = false;
    public string? GoogleId { get; set; }
    public DateTime? LastLoginAt { get; set; }    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public virtual ICollection<UserLogin> UserLogins { get; set; } = [];
}