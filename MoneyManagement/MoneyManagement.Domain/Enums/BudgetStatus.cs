namespace MoneyManagement.Domain.Enums;

/// <summary>
///     Budget status enumeration (EN)<br />
///     Enum trạng thái ngân sách (VI)
/// </summary>
public enum BudgetStatus
{
    /// <summary>
    ///     Budget is active and being tracked (EN)<br />
    ///     Ngân sách đang hoạt động và được theo dõi (VI)
    /// </summary>
    Active = 1,

    /// <summary>
    ///     Budget is paused temporarily (EN)<br />
    ///     Ngân sách tạm dừng (VI)
    /// </summary>
    Paused = 2,

    /// <summary>
    ///     Budget is completed (EN)<br />
    ///     Ngân sách đã hoàn thành (VI)
    /// </summary>
    Completed = 3,

    /// <summary>
    ///     Budget is cancelled (EN)<br />
    ///     Ngân sách đã hủy (VI)
    /// </summary>
    Cancelled = 4
}