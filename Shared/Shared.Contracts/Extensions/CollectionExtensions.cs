namespace Shared.Contracts.Extensions;

/// <summary>
///     Extension methods for collections. (EN)<br />
///     Các phương thức mở rộng cho tập hợp. (VI)
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    ///     Checks if the provided collection is null or empty. (EN)<br />
    ///     Kiểm tra xem tập hợp được cung cấp có null hoặc rỗng hay không. (VI)
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in the collection. (EN)<br />
    ///     Kiểu của các phần tử trong tập hợp. (VI)
    /// </typeparam>
    /// <param name="source">
    ///     The input collection. (EN)<br />
    ///     Tập hợp đầu vào. (VI)
    /// </param>
    /// <returns>
    ///     <c>true</c> if the collection is null or empty; otherwise, <c>false</c>. (EN)<br />
    ///     <c>true</c> nếu tập hợp là null hoặc rỗng; ngược lại là <c>false</c>. (VI)
    /// </returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        if (source is IEnumerable<string>) return string.IsNullOrWhiteSpace(source as string);

        return source == null || !source.Any();
    }
}