namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to cancel an expected transaction. (EN)<br />
///     Đại diện cho request hủy một giao dịch dự kiến. (VI)
/// </summary>
public class CancelTransactionRequest
{
    /// <summary>
    ///     The reason for cancelling the expected transaction. (EN)<br />
    ///     Lý do hủy giao dịch dự kiến. (VI)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}