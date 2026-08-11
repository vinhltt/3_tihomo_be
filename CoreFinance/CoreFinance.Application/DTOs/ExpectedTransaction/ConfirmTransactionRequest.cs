namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to confirm an expected transaction. (EN)<br />
///     Đại diện cho request xác nhận một giao dịch dự kiến. (VI)
/// </summary>
public class ConfirmTransactionRequest
{
    /// <summary>
    ///     The ID of the actual transaction to link to the expected transaction. (EN)<br />
    ///     ID của giao dịch thực tế cần liên kết với giao dịch dự kiến. (VI)
    /// </summary>
    public Guid ActualTransactionId { get; set; }
}