namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Specifies the logical operator used to combine filter conditions. (EN)<br />
///     Chỉ định toán tử logic được sử dụng để kết hợp các điều kiện lọc. (VI)
/// </summary>
public enum FilterLogicalOperator
{
    /// <summary>
    ///     Represents the logical AND operator. (EN)<br />
    ///     Đại diện cho toán tử logic AND. (VI)
    /// </summary>
    And = 0,

    /// <summary>
    ///     Represents the logical OR operator. (EN)<br />
    ///     Đại diện cho toán tử logic OR. (VI)
    /// </summary>
    Or = 1
}