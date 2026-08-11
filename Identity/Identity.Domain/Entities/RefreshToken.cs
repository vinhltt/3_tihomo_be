using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

public class RefreshToken : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? RevokedBy { get; set; }
    public DateTime? RevokedAt { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;
}