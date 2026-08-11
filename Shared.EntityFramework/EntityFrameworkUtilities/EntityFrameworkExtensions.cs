using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Shared.Contracts.EntityFrameworkUtilities;

/// <summary>
/// Extension methods for Entity Framework operations
/// </summary>
public static class EntityFrameworkExtensions
{
    /// <summary>
    /// Applies pagination to an IQueryable
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="query">The query to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated query</returns>
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Applies conditional where clause if condition is true
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="query">The query</param>
    /// <param name="condition">The condition to check</param>
    /// <param name="predicate">The predicate to apply if condition is true</param>
    /// <returns>Query with conditional where clause</returns>
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Applies soft delete filter
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="query">The query</param>
    /// <returns>Query with soft delete filter applied</returns>
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> query) where T : class
    {
        // This assumes entities have a DeletedAt property of type DateTime?
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = typeof(T).GetProperty("DeletedAt");
        
        if (property == null || property.PropertyType != typeof(DateTime?))
            return query;

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var nullCheck = Expression.Equal(propertyAccess, Expression.Constant(null, typeof(DateTime?)));
        var lambda = Expression.Lambda<Func<T, bool>>(nullCheck, parameter);

        return query.Where(lambda);
    }

    /// <summary>
    /// Includes soft deleted entities
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="query">The query</param>
    /// <returns>Query including soft deleted entities</returns>
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class
    {
        // This method is used when we want to include soft deleted entities
        // By default, we don't apply any filter
        return query;
    }

    /// <summary>
    /// Orders by creation date descending
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    /// <param name="query">The query</param>
    /// <returns>Query ordered by creation date descending</returns>
    public static IQueryable<T> OrderByNewest<T>(this IQueryable<T> query) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = typeof(T).GetProperty("CreatedAt");
        
        if (property == null)
            return query;

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);
        
        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Orders by creation date ascending
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>    /// <param name="query">The query</param>
    /// <returns>Query ordered by creation date ascending</returns>
    public static IQueryable<T> OrderByOldest<T>(this IQueryable<T> query) where T : class
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = typeof(T).GetProperty("CreatedAt");
        
        if (property == null)
            return query;

        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);
        
        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Converts a queryable to paginated result based on filter request
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="filterRequest">The filter request containing pagination info</param>
    /// <returns>Paginated result</returns>
    public static async Task<BasePaging<T>> ToPagingAsync<T>(this IQueryable<T> query, DTOs.IFilterBodyRequest filterRequest)
    {
        var totalCount = await query.CountAsync();
        
        var pagination = filterRequest.Pagination ?? new DTOs.Pagination { PageIndex = 1, PageSize = 10 };
        var pageIndex = Math.Max(1, pagination.PageIndex);
        var pageSize = Math.Max(1, pagination.PageSize);
        
        var skip = (pageIndex - 1) * pageSize;
        var data = await query.Skip(skip).Take(pageSize).ToListAsync();
        
        return new BasePaging<T>
        {
            Data = data,
            Pagination = new DTOs.Pagination
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalRow = totalCount
            }
        };
    }

    /// <summary>
    /// Converts a queryable to paginated result with explicit pagination parameters
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    /// <param name="query">The queryable to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated result</returns>
    public static async Task<BasePaging<T>> ToPagingAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        var totalCount = await query.CountAsync();
        
        var pageIndex = Math.Max(1, pageNumber);
        var size = Math.Max(1, pageSize);
        
        var skip = (pageIndex - 1) * size;
        var data = await query.Skip(skip).Take(size).ToListAsync();
        
        return new BasePaging<T>
        {
            Data = data,
            Pagination = new DTOs.Pagination
            {
                PageIndex = pageIndex,
                PageSize = size,
                TotalRow = totalCount
            }
        };
    }
}
