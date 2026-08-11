using Identity.Domain.Entities;

namespace Identity.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailExistsAsync(string email, Guid excludeUserId, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> IsUsernameExistsAsync(string username, Guid excludeUserId,
        CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginAt, CancellationToken cancellationToken = default);

    Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, string? search = null,
        CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(string? search = null, CancellationToken cancellationToken = default);
}