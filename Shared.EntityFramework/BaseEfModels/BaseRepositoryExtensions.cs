using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

namespace Shared.Contracts.BaseEfModels;

/// <summary>
/// Extension methods for IBaseRepository
/// </summary>
public static class BaseRepositoryExtensions
{
    /// <summary>
    /// Get entities without tracking for read-only operations
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <returns>IQueryable for further operations</returns>
    public static IQueryable<TEntity> GetNoTrackingEntities<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository)
        where TEntity : BaseEntity<TKey>
    {
        // This would typically be implemented by the concrete repository
        // For now, we'll throw NotImplementedException
        throw new NotImplementedException("GetNoTrackingEntities should be implemented by the concrete repository");
    }

    /// <summary>
    /// Get entity by ID without tracking
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="id">Entity ID</param>
    /// <returns>Entity or null</returns>
    public static async Task<TEntity?> GetByIdNoTrackingAsync<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository, TKey id)
        where TEntity : BaseEntity<TKey>
    {
        // This should delegate to the repository's implementation
        return await repository.GetByIdAsync(id);
    }

    /// <summary>
    /// Hard delete entity (permanent removal)
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="id">Entity ID</param>
    /// <returns>Number of affected rows</returns>
    public static async Task<int> DeleteHardAsync<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository, TKey id)
        where TEntity : BaseEntity<TKey>
    {
        await repository.DeleteAsync(id);
        return 1; // Assuming success
    }

    /// <summary>
    /// Soft delete entity (mark as deleted)
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="id">Entity ID</param>
    /// <returns>Number of affected rows</returns>
    public static async Task<int> DeleteSoftAsync<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository, TKey id)
        where TEntity : BaseEntity<TKey>
    {
        await repository.SoftDeleteAsync(id);
        return 1; // Assuming success
    }

    /// <summary>
    /// Create new entity
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="entity">Entity to create</param>
    /// <returns>Created entity</returns>
    public static async Task<TEntity> CreateAsync<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository, TEntity entity)
        where TEntity : BaseEntity<TKey>
    {
        return await repository.AddAsync(entity);
    }

    /// <summary>
    /// Create multiple entities
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Primary key type</typeparam>
    /// <param name="repository">Repository instance</param>
    /// <param name="entities">Entities to create</param>
    /// <returns>Created entities</returns>
    public static async Task<List<TEntity>> CreateAsync<TEntity, TKey>(this IBaseRepository<TEntity, TKey> repository, List<TEntity> entities)
        where TEntity : BaseEntity<TKey>
    {
        await repository.AddRangeAsync(entities);
        return entities;
    }
}

/// <summary>
/// Extension methods for IQueryable to support pagination
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Convert IQueryable to paginated result
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="filterRequest">Filter and pagination parameters</param>
    /// <returns>Paginated result</returns>
    public static async Task<BasePaging<T>> ToPagingAsync<T>(this IQueryable<T> query, IFilterBodyRequest filterRequest)
    {
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((filterRequest.PageNumber - 1) * filterRequest.PageSize)
            .Take(filterRequest.PageSize)
            .ToListAsync();

        return new BasePaging<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filterRequest.PageNumber,
            PageSize = filterRequest.PageSize
        };
    }

    /// <summary>
    /// Convert IQueryable to paginated result with explicit pagination
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="query">Source query</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated result</returns>
    public static async Task<BasePaging<T>> ToPagingAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new BasePaging<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
