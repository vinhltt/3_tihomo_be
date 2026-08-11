using System.Linq.Expressions;
using Shared.EntityFramework.BaseEfModels;

namespace MoneyManagement.Domain.BaseRepositories;

/// <summary>
///     Base interface for repositories. (EN)<br />
///     Giao diện cơ sở cho các repository. (VI)
/// </summary>
public interface IBaseRepository<TEntity, in TKey> where TEntity : BaseEntity<TKey>
{
    /// <summary>
    ///     Creates a list of new entities asynchronously. (EN)<br />
    ///     Tạo một danh sách các thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entities">
    ///     The list of entities to create. (EN)<br />
    ///     Danh sách các thực thể cần tạo. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> CreateAsync(List<TEntity> entities);

    /// <summary>
    ///     Creates a new entity asynchronously. (EN)<br />
    ///     Tạo một thực thể mới một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entity">
    ///     The entity to create. (EN)<br />
    ///     Thực thể cần tạo. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> CreateAsync(TEntity entity);

    /// <summary>
    ///     Updates an existing entity asynchronously. (EN)<br />
    ///     Cập nhật một thực thể hiện có một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entity">
    ///     The entity to update. (EN)<br />
    ///     Thực thể cần cập nhật. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> UpdateAsync(TEntity entity);

    /// <summary>
    ///     Updates a collection of existing entities asynchronously. (EN)<br />
    ///     Cập nhật một tập hợp các thực thể hiện có một cách bất đồng bộ. (VI)
    /// </summary>
    /// <param name="entities">
    ///     The collection of entities to update. (EN)<br />
    ///     Tập hợp các thực thể cần cập nhật. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> UpdateAsync(IEnumerable<TEntity> entities);

    /// <summary>
    ///     Deletes an entity permanently from the database asynchronously based on its key values. (EN)<br />
    ///     Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu một cách bất đồng bộ dựa trên giá trị khóa của nó. (VI)
    /// </summary>
    /// <param name="keyValues">
    ///     The key values of the entity to delete. (EN)<br />
    ///     Các giá trị khóa của thực thể cần xóa. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> DeleteHardAsync(params object[] keyValues);

    /// <summary>
    ///     Deletes an entity permanently from the database. (EN)<br />
    ///     Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu. (VI)
    /// </summary>
    /// <param name="entity">
    ///     The entity to delete. (EN)<br />
    ///     Thực thể cần xóa. (VI)
    /// </param>
    void DeleteHard(TEntity entity);

    /// <summary>
    ///     Soft deletes an entity asynchronously based on its key values by marking it as deleted. (EN)<br />
    ///     Xóa mềm một thực thể một cách bất đồng bộ dựa trên giá trị khóa của nó bằng cách đánh dấu nó đã bị xóa. (VI)
    /// </summary>
    /// <param name="keyValues">
    ///     The key values of the entity to soft delete. (EN)<br />
    ///     Các giá trị khóa của thực thể cần xóa mềm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> DeleteSoftAsync(params object[] keyValues);

    /// <summary>
    ///     Soft deletes an entity asynchronously by marking it as deleted. (EN)<br />
    ///     Xóa mềm một thực thể một cách bất đồng bộ bằng cách đánh dấu nó đã bị xóa. (VI)
    /// </summary>
    /// <param name="entity">
    ///     The entity to soft delete. (EN)<br />
    ///     Thực thể cần xóa mềm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the database.
    ///     (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa số lượng mục trạng thái đã ghi vào cơ sở dữ liệu. (VI)
    /// </returns>
    Task<int> DeleteSoftAsync(TEntity entity);

    /// <summary>
    ///     Retrieves all entities asynchronously, including specified navigation properties. (EN)<br />
    ///     Truy xuất tất cả các thực thể một cách bất đồng bộ, bao gồm các thuộc tính điều hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing a list of all entities. (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa một danh sách tất cả các thực thể. (VI)
    /// </returns>
    Task<List<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Retrieves an entity by its identifier asynchronously, including specified navigation properties. (EN)<br />
    ///     Truy xuất một thực thể bằng mã định danh của nó một cách bất đồng bộ, bao gồm các thuộc tính điều hướng được chỉ
    ///     định. (VI)
    /// </summary>
    /// <param name="id">
    ///     The identifier of the entity. (EN)<br />
    ///     Mã định danh của thực thể. (VI)
    /// </param>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the entity if found, otherwise null. (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa thực thể nếu tìm thấy, ngược lại là null. (VI)
    /// </returns>
    Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Retrieves an entity by its identifier without tracking asynchronously, including specified navigation properties.
    ///     (EN)<br />
    ///     Truy xuất một thực thể bằng mã định danh của nó mà không theo dõi một cách bất đồng bộ, bao gồm các thuộc tính điều
    ///     hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="id">
    ///     The identifier of the entity. (EN)<br />
    ///     Mã định danh của thực thể. (VI)
    /// </param>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the entity if found, otherwise null. (EN)<br />
    ///     Một task biểu thị hoạt động bất đồng bộ, chứa thực thể nếu tìm thấy, ngược lại là null. (VI)
    /// </returns>
    Task<TEntity?> GetByIdNoTrackingAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Retrieves a queryable table for the entity, including specified navigation properties. (EN)<br />
    ///     Truy xuất một bảng có thể truy vấn cho thực thể, bao gồm các thuộc tính điều hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of entities. (EN)<br />
    ///     Một IQueryable các thực thể. (VI)
    /// </returns>
    IQueryable<TEntity> GetQueryableTable(params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Retrieves entities without tracking, including specified navigation properties. (EN)<br />
    ///     Truy xuất các thực thể không theo dõi, bao gồm các thuộc tính điều hướng được chỉ định. (VI)
    /// </summary>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of entities. (EN)<br />
    ///     Một IQueryable các thực thể. (VI)
    /// </returns>
    IQueryable<TEntity> GetNoTrackingEntities(params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    ///     Retrieves entities without tracking and with identity resolution, including specified navigation properties. (EN)
    ///     <br />
    ///     Truy xuất các thực thể không theo dõi và với độ phân giải định danh, bao gồm các thuộc tính điều hướng được chỉ
    ///     định. (VI)
    /// </summary>
    /// <param name="includes">
    ///     Navigation properties to include. (EN)<br />
    ///     Các thuộc tính điều hướng cần bao gồm. (VI)
    /// </param>
    /// <returns>
    ///     An IQueryable of entities. (EN)<br />
    ///     Một IQueryable các thực thể. (VI)
    /// </returns>
    IQueryable<TEntity> GetNoTrackingEntitiesIdentityResolution(params Expression<Func<TEntity, object>>[] includes);
}