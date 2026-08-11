namespace Shared.Contracts.Constants;

/// <summary>
///     Provides common constant values. (EN)<br />
///     Cung cấp các giá trị hằng số chung. (VI)
/// </summary>
public static class ConstantCommon
{
    /// <summary>
    ///     Represents a constant message for when the page number is below 1. (EN)<br />
    ///     Đại diện cho thông báo hằng số khi số trang nhỏ hơn 1. (VI)
    /// </summary>
    public const string PAGE_NUMBER_BELOW_1 = "Page number cannot be below 1.";

    /// <summary>
    ///     Represents a constant message for when the page size is less than 1. (EN)<br />
    ///     Đại diện cho thông báo hằng số khi kích thước trang nhỏ hơn 1. (VI)
    /// </summary>
    public const string PAGE_SIZE_LESS_THAN_1 = "Page size cannot be less than 1.";

    /// <summary>
    ///     Represents a constant message for when the page index is out of range. (EN)<br />
    ///     Đại diện cho thông báo hằng số khi chỉ mục trang nằm ngoài phạm vi. (VI)
    /// </summary>
    public const string PAGE_INDEX_OUT_OF_RANGE = "Page index out of range.";
}