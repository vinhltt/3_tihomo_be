namespace Identity.Domain.Dtos.Roles;

public record RoleResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> Permissions { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateRoleRequest(
    string Name,
    string Description,
    List<string> Permissions);

public record UpdateRoleRequest(
    string? Name,
    string? Description,
    List<string>? Permissions);

public record AssignUserToRoleRequest(Guid UserId);