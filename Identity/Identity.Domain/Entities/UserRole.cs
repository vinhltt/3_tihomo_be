using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

public class UserRole : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid? AssignedBy { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}