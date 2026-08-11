namespace CoreFinance.Domain.Enums;

/// <summary>
///     Represents the type of financial account. (EN)<br />
///     Biểu thị loại tài khoản tài chính. (VI)
/// </summary>
public enum AccountType
{
    /// <summary>
    ///     Bank account. (EN)<br />
    ///     Tài khoản ngân hàng. (VI)
    /// </summary>
    Bank,

    /// <summary>
    ///     Digital wallet. (EN)<br />
    ///     Ví điện tử. (VI)
    /// </summary>
    Wallet,

    /// <summary>
    ///     Credit card account. (EN)<br />
    ///     Tài khoản thẻ tín dụng. (VI)
    /// </summary>
    CreditCard,

    /// <summary>
    ///     Debit card account. (EN)<br />
    ///     Tài khoản thẻ ghi nợ. (VI)
    /// </summary>
    DebitCard,

    /// <summary>
    ///     Cash account. (EN)<br />
    ///     Tài khoản tiền mặt. (VI)
    /// </summary>
    Cash
}