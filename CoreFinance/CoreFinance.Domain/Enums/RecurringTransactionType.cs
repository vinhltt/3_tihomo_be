namespace CoreFinance.Domain.Enums;

/// <summary>
///     Represents the type of recurring transaction. (EN)<br />
///     Biểu thị loại giao dịch định kỳ. (VI)
/// </summary>
public enum RecurringTransactionType
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
    Transfer
}