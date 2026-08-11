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
    Task<int> SaveChangesAsync();

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
    Task<IDbContextTransaction> BeginTransactionAsync();
}