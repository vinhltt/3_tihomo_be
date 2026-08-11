namespace Shared.EntityFramework.Enums;

/// <summary>
///     Defines the types of filters that can be applied. (EN)<br />
///     Định nghĩa các loại bộ lọc có thể áp dụng. (VI)
/// </summary>
public enum FilterType
{
    /// <summary>
    ///     Equal to the specified value. (EN)<br />
    ///     Bằng giá trị được chỉ định. (VI)
    /// </summary>
    Equal = 0,

    /// <summary>
    ///     Not equal to the specified value. (EN)<br />
    ///     Không bằng giá trị được chỉ định. (VI)
    /// </summary>
    NotEqual = 1,

    /// <summary>
    ///     Starts with the specified string. (EN)<br />
    ///     Bắt đầu bằng chuỗi được chỉ định. (VI)
    /// </summary>
    StartsWith = 2,

    /// <summary>
    ///     Ends with the specified string. (EN)<br />
    ///     Kết thúc bằng chuỗi được chỉ định. (VI)
    /// </summary>
    EndsWith = 3,

    /// <summary>
    ///     Greater than the specified value. (EN)<br />
    ///     Lớn hơn giá trị được chỉ định. (VI)
    /// </summary>
    GreaterThan = 4,

    /// <summary>
    ///     Less than the specified value. (EN)<br />
    ///     Nhỏ hơn giá trị được chỉ định. (VI)
    /// </summary>
    LessThan = 5,

    /// <summary>
    ///     Less than or equal to the specified value. (EN)<br />
    ///     Nhỏ hơn hoặc bằng giá trị được chỉ định. (VI)
    /// </summary>
    LessThanOrEqual = 6,

    /// <summary>
    ///     Greater than or equal to the specified value. (EN)<br />
    ///     Lớn hơn hoặc bằng giá trị được chỉ định. (VI)
    /// </summary>
    GreaterThanOrEqual = 7,

    /// <summary>
    ///     Between the specified values. (EN)<br />
    ///     Nằm giữa các giá trị được chỉ định. (VI)
    /// </summary>
    Between = 8,

    /// <summary>
    ///     Not between the specified values. (EN)<br />
    ///     Không nằm giữa các giá trị được chỉ định. (VI)
    /// </summary>
    NotBetween = 9,

    /// <summary>
    ///     Is not null. (EN)<br />
    ///     Không phải null. (VI)
    /// </summary>
    IsNotNull = 10,

    /// <summary>
    ///     Is null. (EN)<br />
    ///     Là null. (VI)
    /// </summary>
    IsNull = 11,

    /// <summary>
    ///     Is not null or white space. (EN)<br />
    ///     Không phải null hoặc khoảng trắng. (VI)
    /// </summary>
    IsNotNullOrWhiteSpace = 12,

    /// <summary>
    ///     Is null or white space. (EN)<br />
    ///     Là null hoặc khoảng trắng. (VI)
    /// </summary>
    IsNullOrWhiteSpace = 13,

    /// <summary>
    ///     Is empty string. (EN)<br />
    ///     Là chuỗi rỗng. (VI)
    /// </summary>
    IsEmpty = 14,

    /// <summary>
    ///     Is not empty string. (EN)<br />
    ///     Không phải chuỗi rỗng. (VI)
    /// </summary>
    IsNotEmpty = 15,

    /// <summary>
    ///     Is in the specified list of values. (EN)<br />
    ///     Nằm trong danh sách các giá trị được chỉ định. (VI)
    /// </summary>
    In = 16,

    /// <summary>
    ///     Is not in the specified list of values. (EN)<br />
    ///     Không nằm trong danh sách các giá trị được chỉ định. (VI)
    /// </summary>
    NotIn = 17,

    /// <summary>
    ///     Contains the specified substring. (EN)<br />
    ///     Chứa chuỗi con được chỉ định. (VI)
    /// </summary>
    Contains = 18,

    /// <summary>
    ///     Does not contain the specified substring. (EN)<br />
    ///     Không chứa chuỗi con được chỉ định. (VI)
    /// </summary>
    NotContains = 19
}