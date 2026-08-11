using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoneyManagement.Domain.BaseRepositories;
using Shared.EntityFramework.BaseEfModels;

namespace MoneyManagement.Infrastructure.Repositories.Base;

/// <summary>
///     Base class for repositories (EN)<br />
///     Lớp cơ sở cho các repository (VI)
/// </summary>
public class BaseRepository<TEntity, TKey>(
    MoneyManagementDbContext context,
    IHttpContextAccessor? httpContextAccessor
)
    : IBaseRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>
{
    #region ctor

    public BaseRepository(MoneyManagementDbContext context) : this(context, null)
    {
    }

    #endregion ctor

    #region props

    private DbSet<TEntity>? EntitiesDbSet { get; set; }

    #endregion props

    #region public

    /// <summary>
    ///     Retrieves entities without tracking, including specified navigation properties (EN)<br />
    ///     Truy xuất các thực thể không theo dõi, bao gồm các thuộc tính điều hướng được chỉ định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>An IQueryable of entities</returns>
    public IQueryable<TEntity> GetNoTrackingEntities(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTracking();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        query = query.Where(e => e.CreateBy == GetUserNameInHttpContext());
        return query;
    }

    /// <summary>
    ///     Retrieves entities with tracking, including specified navigation properties (EN)<br />
    ///     Truy xuất các thực thể có theo dõi, bao gồm các thuộc tính điều hướng được chỉ định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>An IQueryable of entities</returns>
    public IQueryable<TEntity> GetEntities(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        query = query.Where(e => e.CreateBy == GetUserNameInHttpContext());
        return query;
    }

    /// <summary>
    ///     Gets an entity by its ID asynchronously (EN)<br />
    ///     Lấy một thực thể theo ID của nó một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="id">The entity ID (EN)<br />ID của thực thể (VI)</param>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>The entity if found, otherwise null</returns>
    public async Task<TEntity?> GetByIdAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = GetEntities(includes);
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id));
    }

    /// <summary>
    ///     Gets an entity by its ID without tracking asynchronously (EN)<br />
    ///     Lấy một thực thể theo ID của nó không theo dõi một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="id">The entity ID (EN)<br />ID của thực thể (VI)</param>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>The entity if found, otherwise null</returns>
    public async Task<TEntity?> GetByIdNoTrackingAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        var query = GetNoTrackingEntities(includes);
        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id));
    }

    /// <summary>
    ///     Retrieves all entities asynchronously, including specified navigation properties (EN)<br />
    ///     Truy xuất tất cả các thực thể một cách bất đồng bộ, bao gồm các thuộc tính điều hướng được chỉ định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>A list of all entities</returns>
    public async Task<List<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        return await GetNoTrackingEntities(includes).ToListAsync();
    }

    /// <summary>
    ///     Retrieves a queryable table for the entity, including specified navigation properties (EN)<br />
    ///     Truy xuất một bảng có thể truy vấn cho thực thể, bao gồm các thuộc tính điều hướng được chỉ định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>An IQueryable of entities</returns>
    public IQueryable<TEntity> GetQueryableTable(params Expression<Func<TEntity, object>>[] includes)
    {
        return GetEntities(includes);
    }

    /// <summary>
    ///     Retrieves entities without tracking and with identity resolution, including specified navigation properties (EN)
    ///     <br />
    ///     Truy xuất các thực thể không theo dõi và với độ phân giải định danh, bao gồm các thuộc tính điều hướng được chỉ
    ///     định (VI)
    /// </summary>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>An IQueryable of entities</returns>
    public IQueryable<TEntity> GetNoTrackingEntitiesIdentityResolution(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTrackingWithIdentityResolution();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        query = query.Where(e => e.CreateBy == GetUserNameInHttpContext());
        return query;
    }

    /// <summary>
    ///     Creates a list of new entities asynchronously (EN)<br />
    ///     Tạo một danh sách các thực thể mới một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="entities">The list of entities to create (EN)<br />Danh sách các thực thể cần tạo (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> CreateAsync(List<TEntity> entities)
    {
        await Entities.AddRangeAsync(entities);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Creates a new entity asynchronously (EN)<br />
    ///     Tạo một thực thể mới một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="entity">The entity to create (EN)<br />Thực thể cần tạo (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> CreateAsync(TEntity entity)
    {
        await Entities.AddAsync(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Updates an existing entity (EN)<br />
    ///     Cập nhật một thực thể hiện có (VI)
    /// </summary>
    /// <param name="entity">The entity to update (EN)<br />Thực thể cần cập nhật (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> UpdateAsync(TEntity entity)
    {
        Entities.Update(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Updates a collection of existing entities asynchronously (EN)<br />
    ///     Cập nhật một tập hợp các thực thể hiện có một cách bất đồng bộ (VI)
    /// </summary>
    /// <param name="entities">The collection of entities to update (EN)<br />Tập hợp các thực thể cần cập nhật (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> UpdateAsync(IEnumerable<TEntity> entities)
    {
        Entities.UpdateRange(entities);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Soft deletes an entity by setting the Deleted field (EN)<br />
    ///     Xóa mềm một thực thể bằng cách thiết lập trường Deleted (VI)
    /// </summary>
    /// <param name="entity">The entity to soft delete (EN)<br />Thực thể cần xóa mềm (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> DeleteSoftAsync(TEntity entity)
    {
        entity.IsDeleted = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        Entities.Update(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Soft deletes an entity asynchronously based on its key values by marking it as deleted (EN)<br />
    ///     Xóa mềm một thực thể một cách bất đồng bộ dựa trên giá trị khóa của nó bằng cách đánh dấu nó đã bị xóa (VI)
    /// </summary>
    /// <param name="keyValues">
    ///     The key values of the entity to soft delete (EN)<br />Các giá trị khóa của thực thể cần xóa mềm
    ///     (VI)
    /// </param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> DeleteSoftAsync(params object[] keyValues)
    {
        var entity = await Entities.FindAsync(keyValues);
        if (entity == null) return 0;

        entity.IsDeleted = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        Entities.Update(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Hard deletes an entity from the database (EN)<br />
    ///     Xóa cứng một thực thể khỏi cơ sở dữ liệu (VI)
    /// </summary>
    /// <param name="entity">The entity to hard delete (EN)<br />Thực thể cần xóa cứng (VI)</param>
    public void DeleteHard(TEntity entity)
    {
        Entities.Remove(entity);
    }

    /// <summary>
    ///     Deletes an entity permanently from the database asynchronously based on its key values (EN)<br />
    ///     Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu một cách bất đồng bộ dựa trên giá trị khóa của nó (VI)
    /// </summary>
    /// <param name="keyValues">The key values of the entity to delete (EN)<br />Các giá trị khóa của thực thể cần xóa (VI)</param>
    /// <returns>Number of state entries written to the database</returns>
    public async Task<int> DeleteHardAsync(params object[] keyValues)
    {
        var entity = await Entities.FindAsync(keyValues);
        if (entity == null) return 0;

        Entities.Remove(entity);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    ///     Checks if an entity exists by its ID (EN)<br />
    ///     Kiểm tra xem một thực thể có tồn tại theo ID không (VI)
    /// </summary>
    /// <param name="id">The entity ID (EN)<br />ID của thực thể (VI)</param>
    /// <returns>True if exists, otherwise false</returns>
    public async Task<bool> ExistsAsync(TKey id)
    {
        return await Entities.AnyAsync(e => e.Id!.Equals(id));
    }

    /// <summary>
    ///     Counts entities matching the specified predicate (EN)<br />
    ///     Đếm các thực thể phù hợp với điều kiện được chỉ định (VI)
    /// </summary>
    /// <param name="predicate">The predicate to filter entities (EN)<br />Điều kiện để lọc thực thể (VI)</param>
    /// <returns>The count of matching entities</returns>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
    {
        return predicate == null
            ? await Entities.CountAsync()
            : await Entities.CountAsync(predicate);
    }

    /// <summary>
    ///     Gets entities with pagination support (EN)<br />
    ///     Lấy các thực thể với hỗ trợ phân trang (VI)
    /// </summary>
    /// <param name="pageNumber">Page number (1-based) (EN)<br />Số trang (bắt đầu từ 1) (VI)</param>
    /// <param name="pageSize">Number of items per page (EN)<br />Số mục trên mỗi trang (VI)</param>
    /// <param name="predicate">Optional filter predicate (EN)<br />Điều kiện lọc tùy chọn (VI)</param>
    /// <param name="orderBy">Optional ordering function (EN)<br />Hàm sắp xếp tùy chọn (VI)</param>
    /// <param name="includes">Navigation properties to include (EN)<br />Các thuộc tính điều hướng cần bao gồm (VI)</param>
    /// <returns>Paginated list of entities</returns>
    public async Task<List<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = GetNoTrackingEntities(includes);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    #endregion public

    #region private

    /// <summary>
    ///     Gets the DbSet for the entity type (EN)<br />
    ///     Lấy DbSet cho loại thực thể (VI)
    /// </summary>
    private DbSet<TEntity> Entities => EntitiesDbSet ??= context.Set<TEntity>();

    /// <summary>
    ///     Gets the current user name from HTTP context (EN)<br />
    ///     Lấy tên người dùng hiện tại từ ngữ cảnh HTTP (VI)
    /// </summary>
    /// <returns>User name or empty string if not found</returns>
    private string GetUserNameInHttpContext()
    {
        if (httpContextAccessor?.HttpContext?.User == null) return string.Empty;

        var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim?.Value ?? string.Empty;
    }

    #endregion private
}