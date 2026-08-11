namespace Shared.Contracts.Utilities;

/// <summary>
///     Provides helper methods for working with dictionaries. (EN)<br />
///     Cung cấp các phương thức trợ giúp để làm việc với từ điển. (VI)
/// </summary>
public static class DictionaryHelper
{
    /// <summary>
    ///     Adds a range of key-value pairs from one dictionary to another. If a key already exists in the source dictionary,
    ///     its value is updated. (EN)<br />
    ///     Thêm một loạt các cặp khóa-giá trị từ từ điển này sang từ điển khác. Nếu một khóa đã tồn tại trong từ điển nguồn,
    ///     giá trị của nó sẽ được cập nhật. (VI)
    /// </summary>
    /// <param name="source">
    ///     The source dictionary to add items to. (EN)<br />
    ///     Từ điển nguồn để thêm các mục vào. (VI)
    /// </param>
    /// <param name="dictionaryToAdd">
    ///     The dictionary containing items to add. (EN)<br />
    ///     Từ điển chứa các mục cần thêm. (VI)
    /// </param>
    /// <typeparam name="TKey">The type of the keys in the dictionaries.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionaries.</typeparam>
    /// <returns>
    ///     The source dictionary after adding the items. (EN)<br />
    ///     Từ điển nguồn sau khi thêm các mục. (VI)
    /// </returns>
    public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(
        this Dictionary<TKey, TValue> source,
        Dictionary<TKey, TValue> dictionaryToAdd)
        where TKey : notnull
    {
        foreach (var item in dictionaryToAdd) source[item.Key] = item.Value;

        return source;
    }
}