namespace ExcelApi.Models;

/// <summary>
///     Represents a single row of transaction data extracted from Excel (EN)<br />
///     Đại diện cho một dòng dữ liệu giao dịch được trích xuất từ Excel (VI)
/// </summary>
public class TransactionDataRow
{
    /// <summary>
    ///     The transaction date (EN)<br />
    ///     Ngày giao dịch (VI)
    /// </summary>
    public DateTime? TransactionDate { get; set; }

    /// <summary>
    ///     Description or memo of the transaction (EN)<br />
    ///     Mô tả hoặc ghi chú của giao dịch (VI)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Transaction amount (EN)<br />
    ///     Số tiền giao dịch (VI)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Reference or ID of the transaction (EN)<br />
    ///     Mã tham chiếu hoặc ID của giao dịch (VI)
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    ///     Raw data from Excel row for debugging purposes (EN)<br />
    ///     Dữ liệu thô từ dòng Excel cho mục đích debug (VI)
    /// </summary>
    public Dictionary<string, string> RawData { get; set; } = new();
}