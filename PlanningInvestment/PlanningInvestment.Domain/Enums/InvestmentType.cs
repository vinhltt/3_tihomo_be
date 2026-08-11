namespace PlanningInvestment.Domain.Enums;

/// <summary>
///     Represents the type of investment. (EN)<br />
///     Biểu thị loại đầu tư. (VI)
/// </summary>
public enum InvestmentType
{
    /// <summary>
    ///     Stock investment. (EN)<br />
    ///     Đầu tư cổ phiếu. (VI)
    /// </summary>
    Stock,

    /// <summary>
    ///     Bond investment. (EN)<br />
    ///     Đầu tư trái phiếu. (VI)
    /// </summary>
    Bond,

    /// <summary>
    ///     Mutual fund investment. (EN)<br />
    ///     Đầu tư quỹ tương hỗ. (VI)
    /// </summary>
    MutualFund,

    /// <summary>
    ///     ETF investment. (EN)<br />
    ///     Đầu tư ETF. (VI)
    /// </summary>
    ETF,

    /// <summary>
    ///     Real estate investment. (EN)<br />
    ///     Đầu tư bất động sản. (VI)
    /// </summary>
    RealEstate,

    /// <summary>
    ///     Cryptocurrency investment. (EN)<br />
    ///     Đầu tư tiền điện tử. (VI)
    /// </summary>
    Cryptocurrency,

    /// <summary>
    ///     Commodity investment. (EN)<br />
    ///     Đầu tư hàng hóa. (VI)
    /// </summary>
    Commodity,

    /// <summary>
    ///     Fixed deposit. (EN)<br />
    ///     Tiền gửi có kỳ hạn. (VI)
    /// </summary>
    FixedDeposit,

    /// <summary>
    ///     Other type of investment. (EN)<br />
    ///     Loại đầu tư khác. (VI)
    /// </summary>
    Other
}