namespace CoreFinance.Domain.Enums;

/// <summary>
///     Represents the status of an expected transaction. (EN)<br />
///     Biểu thị trạng thái của một giao dịch dự kiến. (VI)
/// </summary>
public enum ExpectedTransactionStatus
{
    /// <summary>
    ///     Transaction is pending execution. (EN)<br />
    ///     Giao dịch đang chờ thực hiện. (VI)
    /// </summary>
    Pending,

    /// <summary>
    ///     Transaction has been confirmed. (EN)<br />
    ///     Giao dịch đã được xác nhận. (VI)
    /// </summary>
    Confirmed,

    /// <summary>
    ///     Transaction has been cancelled. (EN)<br />
    ///     Giao dịch đã bị hủy bỏ. (VI)
    /// </summary>
    Cancelled,

    /// <summary>
    ///     Transaction has been completed. (EN)<br />
    ///     Giao dịch đã hoàn thành. (VI)
    /// </summary>
    Completed
}