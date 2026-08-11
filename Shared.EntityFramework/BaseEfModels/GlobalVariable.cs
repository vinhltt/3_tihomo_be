namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Represents a global variable holder. (EN)<br />
///     Đại diện cho một bộ chứa biến toàn cục. (VI)
/// </summary>
/// <typeparam name="T">The type of the object held by the global variable.</typeparam>
public class GlobalVariable<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GlobalVariable{T}" /> class with the specified object. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="GlobalVariable{T}" /> với đối tượng được chỉ định. (VI)
    /// </summary>
    /// <param name="object">The object to hold.</param>
    public GlobalVariable(T? @object)
    {
        Object = @object;
    }

    /// <summary>
    ///     Gets the object held by the global variable. (EN)<br />
    ///     Lấy đối tượng được giữ bởi biến toàn cục. (VI)
    /// </summary>
    public static T? Object { get; private set; }
}