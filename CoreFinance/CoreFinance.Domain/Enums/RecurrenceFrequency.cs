namespace CoreFinance.Domain.Enums;

/// <summary>
///     Represents the frequency of recurring transactions. (EN)<br />
///     Biểu thị tần suất của các giao dịch định kỳ. (VI)
/// </summary>
public enum RecurrenceFrequency
{
    /// <summary>
    ///     Custom frequency (use CustomIntervalDays). (EN)<br />
    ///     Tần suất tùy chỉnh (sử dụng CustomIntervalDays). (VI)
    /// </summary>
    Custom = 0,

    /// <summary>
    ///     Daily frequency (every day). (EN)<br />
    ///     Tần suất hàng ngày (mỗi ngày). (VI)
    /// </summary>
    Daily = 1,

    /// <summary>
    ///     Weekly frequency (every 7 days). (EN)<br />
    ///     Tần suất hàng tuần (mỗi 7 ngày). (VI)
    /// </summary>
    Weekly = 7,

    /// <summary>
    ///     Biweekly frequency (every 14 days). (EN)<br />
    ///     Tần suất hai tuần một lần (mỗi 14 ngày). (VI)
    /// </summary>
    Biweekly = 14,

    /// <summary>
    ///     Monthly frequency (every 30 days). (EN)<br />
    ///     Tần suất hàng tháng (mỗi 30 ngày). (VI)
    /// </summary>
    Monthly = 30,

    /// <summary>
    ///     Quarterly frequency (every 90 days). (EN)<br />
    ///     Tần suất hàng quý (mỗi 90 ngày). (VI)
    /// </summary>
    Quarterly = 90,

    /// <summary>
    ///     Semi-annually frequency (every 180 days). (EN)<br />
    ///     Tần suất nửa năm một lần (mỗi 180 ngày). (VI)
    /// </summary>
    SemiAnnually = 180,

    /// <summary>
    ///     Annually frequency (every 365 days). (EN)<br />
    ///     Tần suất hàng năm (mỗi 365 ngày). (VI)
    /// </summary>
    Annually = 365
}