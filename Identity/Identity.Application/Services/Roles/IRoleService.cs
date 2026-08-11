using Identity.Domain.Dtos.Roles;

namespace Identity.Application.Services.Roles;

public interface IRoleService
{
    Task<RoleResponse?> GetByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<RoleResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    Task<RoleResponse> UpdateAsync(Guid roleId, UpdateRoleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task AssignRoleToUserAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default);
    Task RemoveRoleFromUserAsync(Guid roleId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsNameExistsAsync(string name, Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default);
}