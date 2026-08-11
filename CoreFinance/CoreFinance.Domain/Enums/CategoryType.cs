namespace CoreFinance.Domain.Enums;

/// <summary>
///     Represents the category type of a transaction. (EN)<br />
///     Biểu thị loại danh mục của giao dịch. (VI)
/// </summary>
public enum CategoryType
{
    /// <summary>
    ///     Income transaction. (EN)<br />
    ///     Giao dịch thu nhập. (VI)
    /// </summary>
    Income,

    /// <summary>
    ///     Expense transaction. (EN)<br />
    ///     Giao dịch chi tiêu. (VI)
    /// </summary>
    Expense,

    /// <summary>
    ///     Transfer transaction. (EN)<br />
    ///     Giao dịch chuyển khoản. (VI)
    /// </summary>
    Transfer,

    /// <summary>
    ///     Fee transaction. (EN)<br />
    ///     Giao dịch phí. (VI)
    /// </summary>
    Fee,

    /// <summary>
    ///     Other type of transaction. (EN)<br />
    ///     Loại giao dịch khác. (VI)
    /// </summary>
    Other
}