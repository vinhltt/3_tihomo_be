using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shared.EntityFramework.BaseEfModels;
using MoneyManagement.Domain.BaseRepositories;
using MoneyManagement.Domain.UnitOfWorks;

namespace MoneyManagement.Infrastructure.UnitOfWorks;

/// <summary>
///     Represents a Unit of Work implementation using Entity Framework Core (EN)<br />
///     Biểu thị một triển khai Unit of Work sử dụng Entity Framework Core (VI)
/// </summary>
/// <typeparam name="TContext">The type of the DbContext (EN)<br />Kiểu của DbContext (VI)</typeparam>
public class UnitOfWork<TContext>(
    TContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork
    where TContext : DbContext
{
    private bool _disposed;
    private Dictionary<Type, object?>? _repositories;

    /// <summary>
    ///     Saves all changes made in this context to the database asynchronously (EN)<br />
    ///     Lưu tất cả các thay đổi được thực hiện trong ngữ cảnh này vào cơ sở dữ liệu một cách bất đồng bộ (VI)
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database
    /// </returns>
    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    /// <summary>
    ///     Begins a new database transaction asynchronously (EN)<br />
    ///     Bắt đầu một giao dịch cơ sở dữ liệu mới một cách bất đồng bộ (VI)
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created transaction</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }

    /// <summary>
    ///     Gets the content of a template query file asynchronously (EN)<br />
    ///     Lấy nội dung của tệp truy vấn mẫu một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="methodName">The name of the calling method (EN)<br />Tên của phương thức gọi (VI)</param>
    /// <param name="path">The file path of the calling member (EN)<br />Đường dẫn tệp của thành viên gọi (VI)</param>
    /// <returns>A task representing the asynchronous operation, containing the content of the query file</returns>
    /// <exception cref="FileNotFoundException">
    ///     Thrown if the query file is not found (EN)<br />Ném ngoại lệ nếu tệp truy vấn
    ///     không được tìm thấy (VI)
    /// </exception>
    public async Task<string> GetTemplateQueryAsync(
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string path = ""
    )
    {
        var queryFilePath = BuildPathDao(path, methodName);
        return await File.ReadAllTextAsync(queryFilePath);
    }

    /// <summary>
    ///     Gets a repository for the specified entity type (EN)<br />
    ///     Lấy một repository cho kiểu thực thể được chỉ định (VI)
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity (EN)<br />Kiểu của thực thể (VI)</typeparam>
    /// <typeparam name="TKey">The type of the entity's key (EN)<br />Kiểu của khóa thực thể (VI)</typeparam>
    /// <returns>An instance of the base repository for the specified entity type</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the repository service is not registered (EN)<br />Ném ngoại lệ
    ///     nếu dịch vụ repository chưa được đăng ký (VI)
    /// </exception>
    public IBaseRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : BaseEntity<TKey>
    {
        _repositories ??= new Dictionary<Type, object?>();

        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
            _repositories[type] = serviceProvider.GetService<IBaseRepository<TEntity, TKey>>();
        return _repositories[type] as IBaseRepository<TEntity, TKey> ??
               throw new InvalidOperationException();
    }

    /// <summary>
    ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources (EN)<br />
    ///     Thực hiện các tác vụ do ứng dụng định nghĩa liên quan đến việc giải phóng, phát hành hoặc đặt lại các tài nguyên
    ///     không được quản lý (VI)
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizes an instance of the UnitOfWork class (EN)<br />
    ///     Kết thúc một phiên bản của lớp UnitOfWork (VI)
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(false);
    }

    /// <summary>
    ///     Releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources (EN)<br />
    ///     Giải phóng các tài nguyên không được quản lý được sử dụng bởi UnitOfWork và tùy chọn giải phóng các tài nguyên được
    ///     quản lý (VI)
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) context.Dispose();
            _disposed = true;
        }
    }

    #region private

    /// <summary>
    ///     Builds the path for DAO query files (EN)<br />
    ///     Xây dựng đường dẫn cho các tệp truy vấn DAO (VI)
    /// </summary>
    /// <param name="path">The file path (EN)<br />Đường dẫn tệp (VI)</param>
    /// <param name="methodName">The method name (EN)<br />Tên phương thức (VI)</param>
    /// <returns>The constructed file path</returns>
    private static string BuildPathDao(string path, string methodName)
    {
        var directory = Path.GetDirectoryName(path);
        var fileName = Path.GetFileNameWithoutExtension(path);
        return Path.Combine(directory!, $"{fileName}.{methodName}.sql");
    }

    #endregion private
}