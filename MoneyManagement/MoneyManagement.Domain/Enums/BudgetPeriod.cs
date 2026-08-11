namespace MoneyManagement.Domain.Enums;

/// <summary>
///     Budget period enumeration (EN)<br />
///     Enum chu kỳ ngân sách (VI)
/// </summary>
public enum BudgetPeriod
{
    /// <summary>
    ///     Weekly budget (EN)<br />
    ///     Ngân sách hàng tuần (VI)
    /// </summary>
    Weekly = 1,

    /// <summary>
    ///     Monthly budget (EN)<br />
    ///     Ngân sách hàng tháng (VI)
    /// </summary>
    Monthly = 2,

    /// <summary>
    ///     Quarterly budget (EN)<br />
    ///     Ngân sách hàng quý (VI)
    /// </summary>
    Quarterly = 3,

    /// <summary>
    ///     Yearly budget (EN)<br />
    ///     Ngân sách hàng năm (VI)
    /// </summary>
    Yearly = 4,

    /// <summary>
    ///     Custom period budget (EN)<br />
    ///     Ngân sách chu kỳ tùy chỉnh (VI)
    /// </summary>
    Custom = 5
}