using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

/// <summary>
/// Represents external login provider information for a user
/// Lưu trữ thông tin đăng nhập từ các nhà cung cấp bên ngoài cho người dùng
/// </summary>
public class UserLogin : BaseEntity<Guid>
{
    /// <summary>
    /// User ID that this login belongs to
    /// ID người dùng mà thông tin đăng nhập này thuộc về
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Login provider name (e.g., "Google", "Facebook")
    /// Tên nhà cung cấp đăng nhập (ví dụ: "Google", "Facebook")
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// User ID from the external provider
    /// ID người dùng từ nhà cung cấp bên ngoài
    /// </summary>
    public string ProviderUserId { get; set; } = string.Empty;

    /// <summary>
    /// Display name from the provider (optional)
    /// Tên hiển thị từ nhà cung cấp (tùy chọn)
    /// </summary>
    public string? ProviderDisplayName { get; set; }

    /// <summary>
    /// Last time this login was used
    /// Lần cuối cùng thông tin đăng nhập này được sử dụng
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    /// <summary>
    /// The user this login belongs to
    /// Người dùng mà thông tin đăng nhập này thuộc về
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;
}
