namespace PlanningInvestment.Domain.Enums;

/// <summary>
///     Represents the status of a financial goal. (EN)<br />
///     Biểu thị trạng thái của mục tiêu tài chính. (VI)
/// </summary>
public enum GoalStatus
{
    /// <summary>
    ///     Goal is in planning phase. (EN)<br />
    ///     Mục tiêu đang trong giai đoạn lập kế hoạch. (VI)
    /// </summary>
    Planning,

    /// <summary>
    ///     Goal is active and in progress. (EN)<br />
    ///     Mục tiêu đang hoạt động và tiến hành. (VI)
    /// </summary>
    Active,

    /// <summary>
    ///     Goal is paused. (EN)<br />
    ///     Mục tiêu đang tạm dừng. (VI)
    /// </summary>
    Paused,

    /// <summary>
    ///     Goal has been completed. (EN)<br />
    ///     Mục tiêu đã hoàn thành. (VI)
    /// </summary>
    Completed,

    /// <summary>
    ///     Goal has been cancelled. (EN)<br />
    ///     Mục tiêu đã bị hủy bỏ. (VI)
    /// </summary>
    Cancelled
}