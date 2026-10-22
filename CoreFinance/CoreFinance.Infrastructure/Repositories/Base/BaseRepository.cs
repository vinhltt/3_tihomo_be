using System.Linq.Expressions;
using CoreFinance.Domain.BaseRepositories;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Infrastructure.Repositories.Base;

/// <summary>
///     (EN) Base class for repositories. Owner scoping and audit identity come from the scoped
///     <see cref="ICurrentUserContext" />, which fails closed when no owner exists.<br />
///     (VI) Lớp cơ sở cho các repository. Phạm vi owner và danh tính audit lấy từ <see cref="ICurrentUserContext" />
///     theo scope, fail-closed khi không có owner.
/// </summary>
public class BaseRepository<TEntity, TKey>(
    CoreFinanceDbContext context,
    ICurrentUserContext currentUser
)
    : IBaseRepository<TEntity, TKey>
    where TEntity : UserOwnedEntity<TKey>
{
    #region props

    private DbSet<TEntity>? EntitiesDbSet { get; set; }

    #endregion props

    #region public

    /// <summary>
    ///     (EN) Retrieves entities without tracking, including specified navigation properties.<br />
    ///     (VI) Truy xuất các thực thể không theo dõi, bao gồm các thuộc tính điều hướng được chỉ định.
    /// </summary>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>An IQueryable of entities.</returns>
    public IQueryable<TEntity> GetNoTrackingEntities(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTracking();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        var userId = currentUser.UserId;
        query = query.Where(e => e.UserId == userId);
        return query;
    }

    /// <summary>
    ///     (EN) Retrieves entities without tracking and with identity resolution, including specified navigation properties.
    ///     <br />
    ///     (VI) Truy xuất các thực thể không theo dõi và với độ phân giải định danh, bao gồm các thuộc tính điều hướng được
    ///     chỉ định.
    /// </summary>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>An IQueryable of entities.</returns>
    public IQueryable<TEntity> GetNoTrackingEntitiesIdentityResolution(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsNoTrackingWithIdentityResolution();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        var userId = currentUser.UserId;
        query = query.Where(e => e.UserId == userId);
        return query;
    }

    /// <summary>
    ///     (EN) Retrieves a queryable table for the entity, including specified navigation properties.<br />
    ///     (VI) Truy xuất một bảng có thể truy vấn cho thực thể, bao gồm các thuộc tính điều hướng được chỉ định.
    /// </summary>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>An IQueryable of entities.</returns>
    public IQueryable<TEntity> GetQueryableTable(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        var userId = currentUser.UserId;
        query = query.Where(e => e.UserId == userId);
        return query;
    }

    /// <summary>
    ///     (EN) Retrieves all entities asynchronously, including specified navigation properties.<br />
    ///     (VI) Truy xuất tất cả các thực thể một cách bất đồng bộ, bao gồm các thuộc tính điều hướng được chỉ định.
    /// </summary>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>A task representing the asynchronous operation, containing a list of all entities.</returns>
    public virtual Task<List<TEntity>> GetAllAsync(
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = GetNoTrackingEntities();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        var entities = query.ToListAsync();
        return entities;
    }

    /// <summary>
    ///     (EN) Retrieves an entity by its identifier asynchronously, including specified navigation properties.<br />
    ///     (VI) Truy xuất một thực thể bằng mã định danh của nó một cách bất đồng bộ, bao gồm các thuộc tính điều hướng được
    ///     chỉ định.
    /// </summary>
    /// <param name="id">The identifier of the entity. (EN)<br />Mã định danh của thực thể. (VI)</param>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found, otherwise null.</returns>
    public virtual Task<TEntity?> GetByIdAsync(TKey id,
        params Expression<Func<TEntity, object>>[] includes)
    {
        return GetByIdAsync(id, CancellationToken.None, includes);
    }

    /// <summary>
    ///     (EN) Retrieves a tracked entity of the current owner by its identifier asynchronously, including specified
    ///     navigation properties, observing the cancellation token.<br />
    ///     (VI) Truy xuất một thực thể được theo dõi của owner hiện tại bằng mã định danh của nó một cách bất đồng bộ, bao
    ///     gồm các thuộc tính điều hướng được chỉ định, có tôn trọng token hủy.
    /// </summary>
    /// <param name="id">The identifier of the entity. (EN)<br />Mã định danh của thực thể. (VI)</param>
    /// <param name="cancellationToken">A token to cancel the query. (EN)<br />Token dùng để hủy truy vấn. (VI)</param>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found, otherwise null.</returns>
    public virtual Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = Entities.AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        var userId = currentUser.UserId;
        query = query.Where(e => e.UserId == userId);
        var entity = query.SingleOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);
        return entity;
    }

    /// <summary>
    ///     (EN) Retrieves an entity by its identifier without tracking asynchronously, including specified navigation
    ///     properties.<br />
    ///     (VI) Truy xuất một thực thể bằng mã định danh của nó mà không theo dõi một cách bất đồng bộ, bao gồm các thuộc tính
    ///     điều hướng được chỉ định.
    /// </summary>
    /// <param name="id">The identifier of the entity. (EN)<br />Mã định danh của thực thể. (VI)</param>
    /// <param name="includes">Navigation properties to include. (EN)<br />Các thuộc tính điều hướng cần bao gồm. (VI)</param>
    /// <returns>A task representing the asynchronous operation, containing the entity if found, otherwise null.</returns>
    public virtual Task<TEntity?> GetByIdNoTrackingAsync(TKey id,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = GetNoTrackingEntities();
        foreach (var include in includes) query = query.Include(include);
        var entity = query.SingleOrDefaultAsync(x => x.Id!.Equals(id));
        return entity;
    }

    /// <summary>
    ///     (EN) Creates a new entity asynchronously.<br />
    ///     (VI) Tạo một thực thể mới một cách bất đồng bộ.
    /// </summary>
    /// <param name="entity">The entity to create. (EN)<br />Thực thể cần tạo. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual async Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ValidateAndThrow(entity);
        var currentUserName = currentUser.UserName;
        var currentUserId = currentUser.UserId;
        entity.SetDefaultValueWithUser(currentUserName, currentUserId);
        await Entities.AddAsync(entity, cancellationToken);
        var countAffect = await context.SaveChangesAsync(cancellationToken);
        return countAffect;
    }

    /// <summary>
    ///     (EN) Creates a list of new entities asynchronously.<br />
    ///     (VI) Tạo một danh sách các thực thể mới một cách bất đồng bộ.
    /// </summary>
    /// <param name="entities">The list of entities to create. (EN)<br />Danh sách các thực thể cần tạo. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual async Task<int> CreateAsync(List<TEntity> entities)
    {
        ValidateAndThrow(entities);
        var currentUserName = currentUser.UserName;
        var currentUserId = currentUser.UserId;
        entities.ForEach(e => { e.SetDefaultValueWithUser(currentUserName, currentUserId); });

        await Entities.AddRangeAsync(entities);
        var countAffect = await context.SaveChangesAsync();
        return countAffect;
    }

    /// <summary>
    ///     (EN) Updates an existing entity asynchronously.<br />
    ///     (VI) Cập nhật một thực thể hiện có một cách bất đồng bộ.
    /// </summary>
    /// <param name="entity">The entity to update. (EN)<br />Thực thể cần cập nhật. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ValidateAndThrow(entity);
        var currentUserName = currentUser.UserName;
        var entry = context.Entry(entity);
        entity.SetValueUpdate(currentUserName);
        if (entry.State < EntityState.Added) entry.State = EntityState.Modified;
        var countAffect = context.SaveChangesAsync(cancellationToken);
        return countAffect;
    }

    /// <summary>
    ///     (EN) Updates a collection of existing entities asynchronously.<br />
    ///     (VI) Cập nhật một tập hợp các thực thể hiện có một cách bất đồng bộ.
    /// </summary>
    /// <param name="entities">The collection of entities to update. (EN)<br />Tập hợp các thực thể cần cập nhật. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual Task<int> UpdateAsync(IEnumerable<TEntity> entities)
    {
        var currentUserName = currentUser.UserName;
        var baseEntities = entities.ToList();
        baseEntities.ForEach(e =>
        {
            ValidateAndThrow(e);
            e.SetValueUpdate(currentUserName);
        });

        var entry = context.Entry(baseEntities);
        if (entry.State < EntityState.Added) entry.State = EntityState.Modified;
        var countAffect = context.SaveChangesAsync();
        return countAffect;
    }

    /// <summary>
    ///     (EN) Deletes an entity permanently from the database asynchronously based on its key values.<br />
    ///     (VI) Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu một cách bất đồng bộ dựa trên giá trị khóa của nó.
    /// </summary>
    /// <param name="keyValues">The key values of the entity to delete. (EN)<br />Các giá trị khóa của thực thể cần xóa. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown if the number of key values does not match the number of primary key
    ///     properties. (EN)<br />Ném ngoại lệ nếu số lượng giá trị khóa không khớp với số lượng thuộc tính khóa chính. (VI)
    /// </exception>
    public virtual async Task<int> DeleteHardAsync(params object[] keyValues)
    {
        // Logic phức tạp hơn bên trong DeleteHardAsync để xây dựng biểu thức Where động
        // dựa trên keyValues và metadata của khóa chính
        var entityType = context.Model.FindEntityType(typeof(TEntity));
        var primaryKey = entityType?.FindPrimaryKey();

        if (primaryKey == null || primaryKey.Properties.Count != keyValues.Length)
            throw new ArgumentException("Number of key values does not match the number of primary key properties.");

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression? predicateBody = null;

        for (var i = 0; i < primaryKey.Properties.Count; i++)
        {
            var property = primaryKey.Properties[i];
            if (property.PropertyInfo == null) continue;
            var propertyAccess = Expression.MakeMemberAccess(parameter, property.PropertyInfo);
            var keyValue = Expression.Constant(keyValues[i], property.ClrType);
            var equality = Expression.Equal(propertyAccess, keyValue);

            predicateBody = predicateBody == null ? equality : Expression.AndAlso(predicateBody, equality);
        }

        var predicate = Expression.Lambda<Func<TEntity, bool>>(predicateBody!, parameter);

        return await Entities.Where(predicate).ExecuteDeleteAsync();
    }

    /// <summary>
    ///     (EN) Deletes an entity permanently from the database.<br />
    ///     (VI) Xóa vĩnh viễn một thực thể khỏi cơ sở dữ liệu.
    /// </summary>
    /// <param name="entity">The entity to delete. (EN)<br />Thực thể cần xóa. (VI)</param>
    public virtual void DeleteHard(TEntity entity)
    {
        ValidateAndThrow(entity);
        Entities.Remove(entity);
    }

    /// <summary>
    ///     (EN) Soft deletes an entity asynchronously based on its key values by marking it as deleted.<br />
    ///     (VI) Xóa mềm một thực thể một cách bất đồng bộ dựa trên giá trị khóa của nó bằng cách đánh dấu nó đã bị xóa.
    /// </summary>
    /// <param name="keyValues">
    ///     The key values of the entity to soft delete. (EN)<br />Các giá trị khóa của thực thể cần xóa
    ///     mềm. (VI)
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual async Task<int> DeleteSoftAsync(params object[] keyValues)
    {
        var entity = await context.Set<TEntity>().FindAsync(keyValues);
        ValidateAndThrow(entity);
        entity!.IsDeleted = DateTime.Now.ToString("yyyyMMddHHmmss");
        return await UpdateAsync(entity);
    }

    /// <summary>
    ///     (EN) Soft deletes an entity asynchronously by marking it as deleted.<br />
    ///     (VI) Xóa mềm một thực thể một cách bất đồng bộ bằng cách đánh dấu nó đã bị xóa.
    /// </summary>
    /// <param name="entity">The entity to soft delete. (EN)<br />Thực thể cần xóa mềm. (VI)</param>
    /// <returns>
    ///     A task representing the asynchronous operation, containing the number of state entries written to the
    ///     database.
    /// </returns>
    public virtual async Task<int> DeleteSoftAsync(TEntity entity)
    {
        ValidateAndThrow(entity);
        entity.IsDeleted = DateTime.Now.ToString("yyyyMMddHHmmss");
        return await UpdateAsync(entity);
    }

    #endregion public

    #region private

    protected DbSet<TEntity> Entities => EntitiesDbSet ??= context.Set<TEntity>();

    /// <summary>
    ///     (EN) Validates an entity and throws an ArgumentNullException if it is null.<br />
    ///     (VI) Xác thực một thực thể và ném ArgumentNullException nếu nó là null.
    /// </summary>
    /// <param name="entity">The entity to validate. (EN)<br />Thực thể cần xác thực. (VI)</param>
    protected void ValidateAndThrow(TEntity? entity)
    {
        if (entity != null) return;
        throw new ArgumentNullException(nameof(entity));
    }

    /// <summary>
    ///     (EN) Validates a collection of entities and throws an ArgumentNullException if it is null or empty.<br />
    ///     (VI) Xác thực một tập hợp các thực thể và ném ArgumentNullException nếu nó là null hoặc trống.
    /// </summary>
    /// <param name="entities">The collection of entities to validate. (EN)<br />Tập hợp các thực thể cần xác thực. (VI)</param>
    protected void ValidateAndThrow(IEnumerable<TEntity> entities)
    {
        if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
    }

    #endregion private
}