using Shared.EntityFramework.BaseEfModels;

namespace Identity.Domain.Entities;

public class Role : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
}