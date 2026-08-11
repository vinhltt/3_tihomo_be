using Identity.Domain.Dtos.Common;
using Identity.Domain.Dtos.Roles;
using Identity.Domain.Dtos.Users;

namespace Identity.Application.Services.Users;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateAsync(Guid userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PagedResponse<UserResponse>> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null,
        CancellationToken cancellationToken = default);

    Task<bool> ValidatePasswordAsync(Guid userId, string password, CancellationToken cancellationToken = default);

    Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<RoleResponse>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}