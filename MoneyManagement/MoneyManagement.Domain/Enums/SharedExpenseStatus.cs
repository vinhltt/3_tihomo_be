namespace MoneyManagement.Domain.Enums;

/// <summary>
///     Shared expense status enumeration (EN)<br />
///     Enum trạng thái chi tiêu chung (VI)
/// </summary>
public enum SharedExpenseStatus
{
    /// <summary>
    ///     Expense is pending settlement (EN)<br />
    ///     Chi tiêu đang chờ thanh toán (VI)
    /// </summary>
    Pending = 1,

    /// <summary>
    ///     Expense is partially settled (EN)<br />
    ///     Chi tiêu đã thanh toán một phần (VI)
    /// </summary>
    PartiallySettled = 2,

    /// <summary>
    ///     Expense is fully settled (EN)<br />
    ///     Chi tiêu đã thanh toán đầy đủ (VI)
    /// </summary>
    Settled = 3,

    /// <summary>
    ///     Expense is cancelled (EN)<br />
    ///     Chi tiêu đã hủy (VI)
    /// </summary>
    Cancelled = 4
}