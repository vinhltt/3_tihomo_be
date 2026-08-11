namespace PlanningInvestment.Domain.Enums;

/// <summary>
///     Represents the type of debt. (EN)<br />
///     Biểu thị loại khoản nợ. (VI)
/// </summary>
public enum DebtType
{
    /// <summary>
    ///     Credit card debt. (EN)<br />
    ///     Nợ thẻ tín dụng. (VI)
    /// </summary>
    CreditCard,

    /// <summary>
    ///     Personal loan. (EN)<br />
    ///     Vay cá nhân. (VI)
    /// </summary>
    PersonalLoan,

    /// <summary>
    ///     Mortgage loan. (EN)<br />
    ///     Vay mua nhà. (VI)
    /// </summary>
    Mortgage,

    /// <summary>
    ///     Car loan. (EN)<br />
    ///     Vay mua xe. (VI)
    /// </summary>
    CarLoan,

    /// <summary>
    ///     Student loan. (EN)<br />
    ///     Vay học phí. (VI)
    /// </summary>
    StudentLoan,

    /// <summary>
    ///     Business loan. (EN)<br />
    ///     Vay kinh doanh. (VI)
    /// </summary>
    BusinessLoan,

    /// <summary>
    ///     Other type of debt. (EN)<br />
    ///     Loại nợ khác. (VI)
    /// </summary>
    Other
}