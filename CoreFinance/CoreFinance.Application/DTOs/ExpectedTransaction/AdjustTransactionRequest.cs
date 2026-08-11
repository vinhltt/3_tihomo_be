namespace CoreFinance.Application.DTOs.ExpectedTransaction;

/// <summary>
///     Represents a request to adjust an expected transaction amount. (EN)<br />
///     Đại diện cho request điều chỉnh số tiền giao dịch dự kiến. (VI)
/// </summary>
public class AdjustTransactionRequest
{
    /// <summary>
    ///     The new adjusted amount for the expected transaction. (EN)<br />
    ///     Số tiền mới đã điều chỉnh cho giao dịch dự kiến. (VI)
    /// </summary>
    public decimal NewAmount { get; set; }

    /// <summary>
    ///     The reason for the adjustment. (EN)<br />
    ///     Lý do điều chỉnh. (VI)
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}