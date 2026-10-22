using System.Runtime.CompilerServices;
using CoreFinance.Domain.BaseRepositories;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Domain.UnitOfWorks;

/// <summary>
///     Represents the Unit of Work pattern interface for managing transactions and repositories. (EN)<br />
///     Đại diện cho giao diện của Unit of Work pattern để quản lý giao dịch và repository. (VI)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    ///     Asynchronously saves all changes made in this unit of work to the database. (EN)<br />
    ///     Lưu bất đồng bộ tất cả các thay đổi trong unit of work này vào cơ sở dữ liệu. (VI)
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<string> GetTemplateQueryAsync(
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string path = ""
    );

    /// <summary>
    ///     Gets a repository for the specified entity type and key type. (EN)<br />
    ///     Lấy một repository cho kiểu entity và kiểu khóa được chỉ định. (VI)
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the entity's key.</typeparam>
    /// <returns>An instance of IBaseRepository for the specified entity and key type.</returns>
    IBaseRepository<TEntity, TKey> Repository<TEntity, TKey>() where TEntity : UserOwnedEntity<TKey>;

    /// <summary>
    ///     Begins a new database transaction asynchronously. (EN)<br />
    ///     Bắt đầu một giao dịch cơ sở dữ liệu mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the database transaction.</returns>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Takes the transaction-scoped advisory lock for the current owner + resource. Requires the open transaction
    ///     started by <see cref="BeginTransactionAsync" />; callers take it before reading the state they mutate.<br />
    ///     (VI) Lấy advisory lock theo transaction cho owner hiện tại + tài nguyên. Cần transaction đang mở do
    ///     <see cref="BeginTransactionAsync" /> bắt đầu; caller lấy khóa trước khi đọc trạng thái sẽ thay đổi.
    /// </summary>
    /// <remarks>
    ///     (EN) The database releases the lock when the owning transaction commits or rolls back, not when this method returns.
    ///     The owner comes from the current owner context, never from input. It serializes cooperating writers for the
    ///     owner/resource pair; it does not replace owner predicates or unique indexes.<br />
    ///     (VI) Cơ sở dữ liệu nhả khóa khi transaction sở hữu commit hoặc rollback, không phải khi phương thức này trả về.
    ///     Owner lấy từ owner context hiện tại, không bao giờ từ input. Khóa tuần tự hóa các writer cùng tuân thủ cho cặp
    ///     owner/tài nguyên; không thay thế điều kiện lọc owner hoặc unique index.
    /// </remarks>
    /// <param name="resource">(EN) The lock namespace, e.g. the recurring template lock.<br />(VI) Không gian tên của khóa, ví dụ khóa mẫu định kỳ.</param>
    /// <param name="resourceId">(EN) The resource whose mutation is serialized.<br />(VI) Tài nguyên cần tuần tự hóa thay đổi.</param>
    /// <param name="cancellationToken">(EN) Cancels lock acquisition.<br />(VI) Hủy việc chờ lấy khóa.</param>
    /// <returns>(EN) Completes when the transaction holds the lock.<br />(VI) Hoàn tất khi transaction giữ khóa.</returns>
    /// <exception cref="InvalidOperationException">(EN) There is no open transaction.<br />(VI) Không có transaction đang mở.</exception>
    /// <exception cref="OperationCanceledException">(EN) Lock acquisition is cancelled.<br />(VI) Việc lấy khóa bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task AcquireOwnerResourceLockAsync(string resource, Guid resourceId, CancellationToken cancellationToken = default);
}