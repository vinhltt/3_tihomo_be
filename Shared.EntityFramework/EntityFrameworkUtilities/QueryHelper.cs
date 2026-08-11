using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shared.Contracts.DTOs;

namespace Shared.Contracts.EntityFrameworkUtilities;

/// <summary>
/// Utility class for common Entity Framework operations and queries
/// </summary>
public static class QueryHelper
{
    /// <summary>
    /// Applies filtering, sorting, and pagination to a query
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The source query</param>
    /// <param name="filterRequest">Filter and pagination parameters</param>
    /// <returns>Filtered and paginated query</returns>
    public static async Task<(IEnumerable<T> Items, int TotalCount)> ApplyFilterAndPagingAsync<T>(
        this IQueryable<T> query,
        IFilterBodyRequest filterRequest) where T : class
    {        // Apply text search if provided
        if (!string.IsNullOrWhiteSpace(filterRequest.SearchTerm))
        {
            query = ApplyTextSearch(query, filterRequest.SearchTerm);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();        // Apply sorting
        if (filterRequest.Orders != null && filterRequest.Orders.Any())
        {
            // Apply first sort descriptor
            var firstSort = filterRequest.Orders.First();
            query = ApplySorting(query, firstSort.Field, firstSort.Direction.ToString().ToLower());
            
            // Apply additional sort descriptors if any
            for (int i = 1; i < filterRequest.Orders.Count; i++)
            {
                var sort = filterRequest.Orders[i];
                query = ApplySorting(query, sort.Field, sort.Direction.ToString().ToLower());
            }
        }

        // Apply pagination
        if (filterRequest.Pagination != null && filterRequest.Pagination.PageIndex > 0 && filterRequest.Pagination.PageSize > 0)
        {
            query = query.Paginate(filterRequest.Pagination.PageIndex, filterRequest.Pagination.PageSize);
        }

        var items = await query.ToListAsync();
        return (items, totalCount);
    }

    /// <summary>
    /// Applies text search across searchable properties
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The source query</param>
    /// <param name="searchTerm">The search term</param>
    /// <returns>Query with search filter applied</returns>
    private static IQueryable<T> ApplyTextSearch<T>(IQueryable<T> query, string searchTerm) where T : class
    {
        var searchTermLower = searchTerm.ToLower();
        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Get all string properties that can be searched
        var stringProperties = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .ToList();

        if (!stringProperties.Any())
            return query;

        Expression? searchExpression = null;

        foreach (var property in stringProperties)
        {
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            if (toLowerMethod == null || containsMethod == null)
                continue;

            var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
            var containsCall = Expression.Call(propertyToLower, containsMethod, Expression.Constant(searchTermLower));

            // Handle null strings
            var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
            var safeContains = Expression.AndAlso(nullCheck, containsCall);

            searchExpression = searchExpression == null ? safeContains : Expression.OrElse(searchExpression, safeContains);
        }

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Applies dynamic sorting to the query
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The source query</param>
    /// <param name="sortBy">Property name to sort by</param>
    /// <param name="sortDirection">Sort direction (asc/desc)</param>
    /// <returns>Query with sorting applied</returns>
    private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, string? sortDirection) where T : class
    {
        var property = typeof(T).GetProperty(sortBy);
        if (property == null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var lambda = Expression.Lambda(propertyAccess, parameter);

        var isDescending = !string.IsNullOrWhiteSpace(sortDirection) && 
                          sortDirection.ToLower().StartsWith("desc");

        var methodName = isDescending ? "OrderByDescending" : "OrderBy";
        var orderByMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.PropertyType);

        return (IQueryable<T>)orderByMethod.Invoke(null, new object[] { query, lambda })!;
    }

    /// <summary>
    /// Creates a paginated result with metadata
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="items">The items for current page</param>
    /// <param name="totalCount">Total number of items</param>
    /// <param name="pageNumber">Current page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated result with metadata</returns>
    public static BasePaging<T> CreatePaginatedResult<T>(
        IEnumerable<T> items, 
        int totalCount, 
        int pageNumber, 
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
          return new BasePaging<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
