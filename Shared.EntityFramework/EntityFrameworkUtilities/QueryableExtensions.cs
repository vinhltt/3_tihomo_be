using System.Linq.Expressions;
using Shared.EntityFramework.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.EntityFramework.BaseEfModels;
using Shared.Contracts.Constants;
using Shared.Contracts.Extensions;
using Shared.EntityFramework.Enums;

// ReSharper disable All
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.

namespace Shared.EntityFramework.EntityFrameworkUtilities
{
    public static class QueryableExtensions
    {
        /// <summary>
        ///     (EN) Filters a queryable source based on a FilterRequest object.<br />
        ///     (VI) Lọc nguồn có thể truy vấn dựa trên đối tượng FilterRequest.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to filter. (EN)<br />Nguồn có thể truy vấn cần lọc. (VI)</param>
        /// <param name="filter">
        ///     The FilterRequest containing filter details. (EN)<br />FilterRequest chứa thông tin chi tiết về
        ///     lọc. (VI)
        /// </param>
        /// <returns>A new queryable source with the filter applied, or the original source if no filter is provided.</returns>
        public static IQueryable<T> Filter<T>(this IQueryable<T> source, FilterRequest? filter) where T : class
        {
            if (filter == null || filter.Details.IsNullOrEmpty())
            {
                return source;
            }

            var fields = typeof(T).GetProperties();
            var filters = filter.Details!
                .Where(d => fields.Any(f => string.Equals(d.AttributeName, f.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(e => new FilterDescriptor
                {
                    Field = e.AttributeName,
                    Values = e.FilterType == FilterType.In ? e.Value!.Split("|") : [e.Value ?? ""],
                    LogicalOperator = filter.LogicalOperator,
                    Operator = e.FilterType
                }).ToList();
            return source.Where(ExpressionBuilder.Build<T>(filters));
        }

        /// <summary>
        ///     (EN) Determines whether a queryable source is already ordered.<br />
        ///     (VI) Xác định xem nguồn có thể truy vấn đã được sắp xếp hay chưa.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to check. (EN)<br />Nguồn có thể truy vấn cần kiểm tra. (VI)</param>
        /// <returns>
        ///     (EN) <c>true</c> nếu nguồn có thể truy vấn được sắp xếp; ngược lại, <c>false</c>.<br />
        ///     (VI) <c>true</c> nếu nguồn có thể truy vấn đã được sắp xếp; ngược lại, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if source is null.</exception>
        public static bool IsOrdered<T>(this IQueryable<T> source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            return source.Expression.Type.IsAssignableFrom(typeof(IOrderedQueryable<T>));
        }

        #region Where

        /// <summary>
        ///     (EN) Filters data in the given source using a single filter descriptor.<br />
        ///     (VI) Lọc dữ liệu trong nguồn đã cho sử dụng một mô tả lọc duy nhất.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to filter. (EN)<br />Nguồn có thể truy vấn cần lọc. (VI)</param>
        /// <param name="filter">The filter descriptor. (EN)<br />Mô tả lọc. (VI)</param>
        /// <param name="parameterName">
        ///     The name of the parameter in the lambda expression. (EN)<br />Tên của tham số trong biểu
        ///     thức lambda. (VI)
        /// </param>
        /// <returns>A new queryable source with the filter applied.</returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> source, FilterDescriptor filter,
            string parameterName = "x")
            where T : class
        {
            var expression = ExpressionBuilder.Build<T>(filter, parameterName);
            if (expression == null)
                return source;
#if AS_DEBUG
            System.Diagnostics.Debug.WriteLine("Filter Expression: " + expression.Body.ToString());
#endif
            return source.Where(expression);
        }

        /// <summary>
        ///     (EN) Filters data in the given source using a collection of filter descriptors.<br />
        ///     (VI) Lọc dữ liệu trong nguồn đã cho sử dụng một tập hợp các mô tả lọc.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to filter. (EN)<br />Nguồn có thể truy vấn cần lọc. (VI)</param>
        /// <param name="filters">The collection of filter descriptors. (EN)<br />Tập hợp các mô tả lọc. (VI)</param>
        /// <param name="parameterName">
        ///     The name of the parameter in the lambda expression. (EN)<br />Tên của tham số trong biểu
        ///     thức lambda. (VI)
        /// </param>
        /// <returns>A new queryable source with the filters applied.</returns>
        public static IQueryable<T> Where<T>(this IQueryable<T> source, IEnumerable<FilterDescriptor> filters,
            string parameterName = "x")
            where T : class
        {
            var expression = ExpressionBuilder.Build<T>(filters, parameterName);
            if (expression == null)
                return source;
#if AS_DEBUG
            System.Diagnostics.Debug.WriteLine("Filter Expression: " + expression.Body.ToString();
#endif
            return source.Where(expression);
        }

        #endregion

        #region OrderBy

        /// <summary>
        ///     (EN) Orders data in the given source using a single sort descriptor.<br />
        ///     (VI) Sắp xếp dữ liệu trong nguồn đã cho sử dụng một mô tả sắp xếp duy nhất.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to order. (EN)<br />Nguồn có thể truy vấn cần sắp xếp. (VI)</param>
        /// <param name="sort">The sort descriptor. (EN)<br />Mô tả sắp xếp. (VI)</param>
        /// <param name="replaceOrder">
        ///     if set to <c>true</c>, replaces the current order in the source; otherwise, appends to the
        ///     current order. (EN)<br />nếu đặt là <c>true</c>, thay thế thứ tự hiện tại trong nguồn; ngược lại, thêm vào thứ tự
        ///     hiện tại. (VI)
        /// </param>
        /// <returns>A new queryable source with the sort applied, or the original source if no sort is provided.</returns>
        public static IQueryable<T>? OrderBy<T>(this IQueryable<T>? source, SortDescriptor? sort,
            bool replaceOrder = true)
            where T : class
        {
            if (source == null || sort == null)
                return source;
            var parameter = Expression.Parameter(typeof(T));
            var property = Expression.Property(parameter, sort.Field);
            return source.OrderBy(property, parameter, sort.Direction, replaceOrder);
        }

        /// <summary>
        ///     (EN) Orders data in the given source using a collection of sort descriptors.<br />
        ///     (VI) Sắp xếp dữ liệu trong nguồn đã cho sử dụng một tập hợp các mô tả sắp xếp.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to order. (EN)<br />Nguồn có thể truy vấn cần sắp xếp. (VI)</param>
        /// <param name="sorts">The collection of sort descriptors. (EN)<br />Tập hợp các mô tả sắp xếp. (VI)</param>
        /// <param name="replaceOrder">
        ///     if set to <c>true</c>, replaces the current order in the source for the first sort
        ///     descriptor; otherwise, appends to the current order for all sort descriptors. (EN)<br />nếu đặt là <c>true</c>,
        ///     thay thế thứ tự hiện tại trong nguồn cho mô tả sắp xếp đầu tiên; ngược lại, thêm vào thứ tự hiện tại cho tất cả các
        ///     mô tả sắp xếp. (VI)
        /// </param>
        /// <returns>A new queryable source with the sorts applied, or the original source if no sorts are provided.</returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, IEnumerable<SortDescriptor>? sorts,
            bool replaceOrder = true)
            where T : class
        {
            if (source.IsNullOrEmpty() || sorts.IsNullOrEmpty())
            {
                return source;
            }

            var parameter = Expression.Parameter(typeof(T));
            foreach (var item in sorts)
            {
                var property = Expression.Property(parameter, item.Field);
                source = source.OrderBy(property, parameter, item.Direction, replaceOrder);
                replaceOrder = false;
            }

            return source;
        }

        /// <summary>
        ///     (EN) Orders data in the given source by a specific property.<br />
        ///     (VI) Sắp xếp dữ liệu trong nguồn đã cho theo một thuộc tính cụ thể.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="source">The queryable source to order. (EN)<br />Nguồn có thể truy vấn cần sắp xếp. (VI)</param>
        /// <param name="property">
        ///     The MemberExpression representing the property to order by. (EN)<br />MemberExpression biểu thị
        ///     thuộc tính để sắp xếp. (VI)
        /// </param>
        /// <param name="parameter">The parameter expression. (EN)<br />Biểu thức tham số. (VI)</param>
        /// <param name="direction">
        ///     The sort direction (ascending or descending). (EN)<br />Hướng sắp xếp (tăng dần hoặc giảm dần).
        ///     (VI)
        /// </param>
        /// <param name="replaceOrder">
        ///     if set to <c>true</c>, replaces the current order in the source; otherwise, appends to the
        ///     current order. (EN)<br />nếu đặt là <c>true</c>, thay thế thứ tự hiện tại trong nguồn; ngược lại, thêm vào thứ tự
        ///     hiện tại. (VI)
        /// </param>
        /// <returns>A new queryable source with the sort applied.</returns>
        private static IQueryable<T> OrderBy<T>(this IQueryable<T> source, MemberExpression property,
            ParameterExpression parameter, SortDirection direction, bool replaceOrder = true)
            where T : class
        {
            var propertyType = property.Type;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            var isNullable = underlyingType != null;
            if (propertyType == typeof(string))
            {
                return source.OrderBy<T, string>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(DateTime) || isNullable && underlyingType == typeof(DateTime))
            {
                if (isNullable)
                    return source.OrderBy<T, DateTime?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, DateTime>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(int) || isNullable && underlyingType == typeof(int))
            {
                if (isNullable)
                    return source.OrderBy<T, int?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, int>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(long) || isNullable && underlyingType == typeof(long))
            {
                if (isNullable)
                    return source.OrderBy<T, long?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, long>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(bool) || isNullable && underlyingType == typeof(bool))
            {
                if (isNullable)
                    return source.OrderBy<T, bool?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, bool>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(Guid) || isNullable && underlyingType == typeof(Guid))
            {
                if (isNullable)
                    return source.OrderBy<T, Guid?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, Guid>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(TimeSpan) || isNullable && underlyingType == typeof(TimeSpan))
            {
                if (isNullable)
                    return source.OrderBy<T, TimeSpan?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, TimeSpan>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(DateTimeOffset) || isNullable && underlyingType == typeof(DateTimeOffset))
            {
                if (isNullable)
                    return source.OrderBy<T, DateTimeOffset?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, DateTimeOffset>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(byte) || isNullable && underlyingType == typeof(byte))
            {
                if (isNullable)
                    return source.OrderBy<T, byte?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, byte>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(float) || isNullable && underlyingType == typeof(float))
            {
                if (isNullable)
                    return source.OrderBy<T, float?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, float>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(double) || isNullable && underlyingType == typeof(double))
            {
                if (isNullable)
                    return source.OrderBy<T, double?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, double>(property, parameter, direction, replaceOrder);
            }

            if (propertyType == typeof(decimal) || isNullable && underlyingType == typeof(decimal))
            {
                if (isNullable)
                    return source.OrderBy<T, decimal?>(property, parameter, direction, replaceOrder);
                return source.OrderBy<T, decimal>(property, parameter, direction, replaceOrder);
            }

            return source.OrderBy<T, dynamic>(property, parameter, direction, replaceOrder);
        }

        /// <summary>
        ///     (EN) Orders data in the given source by a specific property of a specified type.<br />
        ///     (VI) Sắp xếp dữ liệu trong nguồn đã cho theo một thuộc tính cụ thể thuộc kiểu được chỉ định.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <typeparam name="TProperty">The type of the property to order by. (EN)<br />Kiểu của thuộc tính để sắp xếp. (VI)</typeparam>
        /// <param name="source">The queryable source to order. (EN)<br />Nguồn có thể truy vấn cần sắp xếp. (VI)</param>
        /// <param name="property">
        ///     The MemberExpression representing the property to order by. (EN)<br />MemberExpression biểu thị
        ///     thuộc tính để sắp xếp. (VI)
        /// </param>
        /// <param name="parameter">The parameter expression. (EN)<br />Biểu thức tham số. (VI)</param>
        /// <param name="direction">
        ///     The sort direction (ascending or descending). (EN)<br />Hướng sắp xếp (tăng dần hoặc giảm dần).
        ///     (VI)
        /// </param>
        /// <param name="replaceOrder">
        ///     if set to <c>true</c>, replaces the current order in the source; otherwise, appends to the
        ///     current order. (EN)<br />nếu đặt là <c>true</c>, thay thế thứ tự hiện tại trong nguồn; ngược lại, thêm vào thứ tự
        ///     hiện tại. (VI)
        /// </param>
        /// <returns>A new queryable source with the sort applied.</returns>
        private static IOrderedQueryable<T> OrderBy<T, TProperty>(this IQueryable<T> source, MemberExpression property,
            ParameterExpression parameter, SortDirection direction, bool replaceOrder = true)
        {
            var expression = Expression.Lambda<Func<T, TProperty>>(property, parameter);
            if (replaceOrder || !source.Expression.Type.IsAssignableFrom(typeof(IOrderedQueryable<T>)) ||
                source is not IOrderedQueryable<T> orderedQueryable)
                return direction == SortDirection.Asc
                    ? source.OrderBy(expression)
                    : source.OrderByDescending(expression);
            return direction == SortDirection.Asc
                ? orderedQueryable.ThenBy(expression)
                : orderedQueryable.ThenByDescending(expression);
        }

        #endregion

        #region Paging

        /// <summary>
        ///     (EN) Converts a queryable source to a paginated result asynchronously.<br />
        ///     (VI) Chuyển đổi nguồn có thể truy vấn thành kết quả phân trang một cách bất đồng bộ.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="queryable">The queryable source to paginate. (EN)<br />Nguồn có thể truy vấn cần phân trang. (VI)</param>
        /// <param name="pagination">The pagination details. (EN)<br />Thông tin chi tiết về phân trang. (VI)</param>
        /// <returns>A task representing the asynchronous operation, containing the paginated result.</returns>
        /// <exception cref="ArgumentNullException">Thrown if queryable or pagination is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if pageIndex or pageSize is less than 1, or if pageIndex is out of
        ///     range.
        /// </exception>
        public static async Task<IBasePaging<TSource>> ToPagingAsync<TSource>(this IQueryable<TSource> queryable,
            Pagination pagination)
        {
            ArgumentNullException.ThrowIfNull(pagination, nameof(pagination));
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

            var pageIndex = pagination.PageIndex;
            if (pageIndex < 1)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(pageIndex),
                    actualValue: pageIndex,
                    message: ConstantCommon.PAGE_NUMBER_BELOW_1
                );
            }

            var pageSize = pagination.PageSize;
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    pageSize,
                    ConstantCommon.PAGE_SIZE_LESS_THAN_1
                );
            }

            var result = new BasePaging<TSource>();
            var skip = (pageIndex - 1) * pageSize;
            var totalRow = await queryable.CountAsync();
            if (totalRow > 0 && totalRow <= skip)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(pageSize),
                    actualValue: pageSize,
                    message: ConstantCommon.PAGE_INDEX_OUT_OF_RANGE
                );
            }

            result.Data = await queryable
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
            var newPagination = new Pagination
            {
                TotalRow = totalRow,
                PageCount = totalRow > 0 ? (int)Math.Ceiling(totalRow / (double)pageSize) : 0,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            result.Pagination = newPagination;
            return result;
        }

        /// <summary>
        ///     (EN) Filters, orders, and paginates a queryable source based on a FilterBodyRequest asynchronously.<br />
        ///     (VI) Lọc, sắp xếp và phân trang nguồn có thể truy vấn dựa trên FilterBodyRequest một cách bất đồng bộ.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="queryable">The queryable source. (EN)<br />Nguồn có thể truy vấn. (VI)</param>
        /// <param name="request">
        ///     The FilterBodyRequest containing filtering, sorting, and pagination details. (EN)<br />
        ///     FilterBodyRequest chứa thông tin chi tiết về lọc, sắp xếp và phân trang. (VI)
        /// </param>
        /// <returns>A task representing the asynchronous operation, containing the paginated result.</returns>
        public static async Task<IBasePaging<TSource>> ToPagingAsync<TSource>(this IQueryable<TSource> queryable,
            IFilterBodyRequest request)
            where TSource : class
        {
            if (request.Filter != null && !request.Filter.Details.IsNullOrEmpty())
                queryable = queryable.Filter(request.Filter);

            if (!request.Orders.IsNullOrEmpty())
                queryable = queryable.OrderBy(request.Orders);

            if (request.Pagination == null || request.Pagination.PageIndex < 1)
            {
                request.Pagination = new Pagination();
            }

            var result = await queryable.ToPagingAsync(request.Pagination);

            return result;
        }

        /// <summary>
        ///     (EN) Filters, orders, and paginates a queryable source and selects a result type based on a FilterBodyRequest and a
        ///     selector asynchronously.<br />
        ///     (VI) Lọc, sắp xếp và phân trang nguồn có thể truy vấn, đồng thời chọn kiểu kết quả dựa trên FilterBodyRequest và
        ///     selector một cách bất đồng bộ.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements in the result. (EN)<br />Kiểu của các phần tử trong kết quả. (VI)</typeparam>
        /// <typeparam name="TSource">The type of the elements in the source. (EN)<br />Kiểu của các phần tử trong nguồn. (VI)</typeparam>
        /// <param name="queryable">The queryable source. (EN)<br />Nguồn có thể truy vấn. (VI)</param>
        /// <param name="request">
        ///     The FilterBodyRequest containing filtering, sorting, and pagination details. (EN)<br />
        ///     FilterBodyRequest chứa thông tin chi tiết về lọc, sắp xếp và phân trang. (VI)
        /// </param>
        /// <param name="selector">
        ///     The selector expression to transform the source elements into the result type. (EN)<br />Biểu
        ///     thức selector để chuyển đổi các phần tử nguồn thành kiểu kết quả. (VI)
        /// </param>
        /// <returns>A task representing the asynchronous operation, containing the paginated result of the specified result type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if request, request.Pagination, or selector is null.</exception>
        public static async Task<IBasePaging<TResult>> ToPagingAsync<TResult, TSource>(
            this IQueryable<TSource> queryable, IFilterBodyRequest request, Expression<Func<TSource, TResult>> selector)
            where TResult : class
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));
            ArgumentNullException.ThrowIfNull(request.Pagination, nameof(request.Pagination));
            ArgumentNullException.ThrowIfNull(selector, nameof(selector));

            var viewModelQuery = queryable.Select(selector);

            var result = await viewModelQuery.ToPagingAsync(request);

            return result;
        }

        #endregion
    }
}