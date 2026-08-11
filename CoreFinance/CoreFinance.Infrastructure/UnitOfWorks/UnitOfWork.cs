using System.Reflection;
using System.Runtime.CompilerServices;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Shared.Contracts.Attributes;
using Shared.EntityFramework.BaseEfModels;
using Shared.Contracts.Constants;
using Shared.Contracts.Utilities;

namespace CoreFinance.Infrastructure.UnitOfWorks;

/// <summary>
///     (EN) Represents a Unit of Work implementation using Entity Framework Core.<br />
///     (VI) Biểu thị một triển khai Unit of Work sử dụng Entity Framework Core.
/// </summary>
/// <typeparam name="TContext">
///     The type of the DbContext. (EN)<br />
///     Kiểu của DbContext. (VI)
/// </typeparam>
public class UnitOfWork<TContext>(
    TContext context,
    IServiceProvider serviceProvider)
    : IUnitOfWork
    where TContext : DbContext
{
    private bool _disposed;
    private Dictionary<Type, object?>? _repositories;

    /// <summary>
    ///     (EN) Saves all changes made in this context to the database asynchronously.<br />
    ///     (VI) Lưu tất cả các thay đổi được thực hiện trong ngữ cảnh này vào cơ sở dữ liệu một cách bất đồng bộ.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    /// <summary>
    ///     (EN) Begins a new database transaction asynchronously.<br />
    ///     (VI) Bắt đầu một giao dịch cơ sở dữ liệu mới một cách bất đồng bộ.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
    /// </returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await context.Database.BeginTransactionAsync();
    }

    /// <summary>
    ///     (EN) Gets the content of a template query file asynchronously.<br />
    ///     (VI) Lấy nội dung của tệp truy vấn mẫu một cách bất đồng bộ.
    /// </summary>
    /// <param name="methodName">
    ///     The name of the calling method. (EN)<br />
    ///     Tên của phương thức gọi. (VI)
    /// </param>
    /// <param name="path">
    ///     The file path of the calling member. (EN)<br />
    ///     Đường dẫn tệp của thành viên gọi. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the content of the query file.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    ///     Thrown if the query file is not found. (EN)<br />
    ///     Ném ngoại lệ nếu tệp truy vấn không được tìm thấy. (VI)
    /// </exception>
    public async Task<string> GetTemplateQueryAsync(
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string path = ""
    )
    {
        var queryFilePath = BuildPathDao(path,
            methodName);

        return await File.ReadAllTextAsync(queryFilePath);
    }

    /// <summary>
    ///     (EN) Gets a repository for the specified entity type.<br />
    ///     (VI) Lấy một repository cho kiểu thực thể được chỉ định.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the entity. (EN)<br />
    ///     Kiểu của thực thể. (VI)
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     The type of the entity's key. (EN)<br />
    ///     Kiểu của khóa thực thể. (VI)
    /// </typeparam>
    /// <returns>An instance of the base repository for the specified entity type.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the repository service is not registered. (EN)<br />
    ///     Ném ngoại lệ nếu dịch vụ repository chưa được đăng ký. (VI)
    /// </exception>
    public IBaseRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : UserOwnedEntity<TKey>
    {
        _repositories ??= new Dictionary<Type, object?>();

        var type = typeof(TEntity);
        if (!_repositories.ContainsKey(type))
            _repositories[type] = serviceProvider.GetService<IBaseRepository<TEntity, TKey>>();
        return _repositories[type] as IBaseRepository<TEntity, TKey> ??
               throw new InvalidOperationException();
    }

    /// <summary>
    ///     (EN) Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.<br />
    ///     (VI) Thực hiện các tác vụ do ứng dụng định nghĩa liên quan đến việc giải phóng, phát hành hoặc đặt lại các tài
    ///     nguyên không được quản lý.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     (EN) Finalizes an instance of the <see cref="UnitOfWork{TContext}" /> class.<br />
    ///     (VI) Kết thúc một phiên bản của lớp <see cref="UnitOfWork{TContext}" />.
    /// </summary>
    ~UnitOfWork()
    {
        Dispose(false);
    }

    /// <summary>
    ///     (EN) Releases the unmanaged resources used by the UnitOfWork and optionally releases the managed resources.<br />
    ///     (VI) Giải phóng các tài nguyên không được quản lý được sử dụng bởi UnitOfWork và tùy chọn giải phóng các tài nguyên
    ///     được quản lý.
    /// </summary>
    /// <param name="disposing">
    ///     True to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources. (EN)<br />True để giải phóng cả tài nguyên được quản lý và không được quản lý; false chỉ để giải phóng
    ///     tài nguyên không được quản lý. (VI)
    /// </param>
    protected virtual void Dispose(
        bool disposing
    )
    {
        if (!_disposed)
        {
            if (disposing) context.Dispose();

            // Free unmanaged resources (unmanaged objects) and override finalizer
            // Set large fields to null
            _disposed = true;
        }
    }

    /// <summary>
    ///     (EN) Builds the file path for a DAO query template.<br />
    ///     (VI) Xây dựng đường dẫn tệp cho một mẫu truy vấn DAO.
    /// </summary>
    /// <param name="directory">The directory containing the query file. (EN)<br />Thư mục chứa tệp truy vấn. (VI)</param>
    /// <param name="methodName">
    ///     The name of the method, used to determine the query file name. (EN)<br />Tên của phương thức,
    ///     được sử dụng để xác định tên tệp truy vấn. (VI)
    /// </param>
    /// <returns>The full path to the query file.</returns>
    /// <exception cref="FileNotFoundException">
    ///     Thrown if the constructed query file path does not exist. (EN)<br />Ném ngoại
    ///     lệ nếu đường dẫn tệp truy vấn được xây dựng không tồn tại. (VI)
    /// </exception>
    protected string BuildPathDao(
        string directory,
        string methodName
    )
    {
        if (methodName.EndsWith(CommonConst.ASYNC))
            methodName = methodName[..^CommonConst.ASYNC.Length];

        var queryFilePath = Path.Combine(FileHelper.GetApplicationFolder(),
            EnvironmentConst.RESOURCES_FOLDER,
            Path.GetFileNameWithoutExtension(directory),
            $"{methodName}.sql");

        if (!File.Exists(queryFilePath))
            throw new FileNotFoundException(queryFilePath);

        return queryFilePath;
    }

    /// <summary>
    ///     (EN) Builds an array of NpgsqlParameter objects from the properties of an input object decorated with
    ///     SqlParameterAttribute.<br />
    ///     (VI) Xây dựng một mảng các đối tượng NpgsqlParameter từ các thuộc tính của đối tượng đầu vào được trang trí bằng
    ///     SqlParameterAttribute.
    /// </summary>
    /// <typeparam name="T">The type of the input object. (EN)<br />Kiểu của đối tượng đầu vào. (VI)</typeparam>
    /// <param name="input">The input object. (EN)<br />Đối tượng đầu vào. (VI)</param>
    /// <returns>An array of NpgsqlParameter objects.</returns>
    public object[] BuildEfParameters<T>(
        T input
    )
        where T : class
    {
        var result = typeof(T).GetProperties()
            .Select(prop => GetSqlParameterAttributeValue(input, prop))
            .Where(param => param != null)
            .Select(param => param!)
            .ToArray();

        return result;
    }

    /// <summary>
    ///     (EN) Gets the NpgsqlParameter value for a property if it is decorated with SqlParameterAttribute.<br />
    ///     (VI) Lấy giá trị NpgsqlParameter cho một thuộc tính nếu nó được trang trí bằng SqlParameterAttribute.
    /// </summary>
    /// <typeparam name="T">The type of the input object. (EN)<br />Kiểu của đối tượng đầu vào. (VI)</typeparam>
    /// <param name="input">The input object. (EN)<br />Đối tượng đầu vào. (VI)</param>
    /// <param name="prop">The PropertyInfo of the property. (EN)<br />PropertyInfo của thuộc tính. (VI)</param>
    /// <returns>An NpgsqlParameter object if the attribute is present and the value is retrieved; otherwise, null.</returns>
    public virtual object? GetSqlParameterAttributeValue<T>(
        T input,
        PropertyInfo prop
    )
    {
        var type = input!.GetType()
            .GetProperty(prop.Name);

        var attribute = type!.GetCustomAttributes(typeof(SqlParameterAttribute),
                true)
            .FirstOrDefault();

        if (attribute == null)
            return null;

        var description = (SqlParameterAttribute)attribute;

        var value = prop.GetValue(input);

        var result = new NpgsqlParameter
        {
            DbType = description.DbType,
            ParameterName = description.ParameterName,
            Value = value
        };

        return result;
    }
}