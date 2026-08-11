namespace CoreFinance.Contracts.Messages;

/// <summary>
/// Message received when transaction data is uploaded from Excel (EN)<br/>
/// Message nhận được khi transaction data được upload từ Excel (VI)
/// </summary>
public class UploadTransactionDataMessage
{
    /// <summary>
    /// Correlation ID để tracking request (EN)<br/>
    /// Correlation ID để tracking request (VI)
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Tên file Excel được upload (EN)<br/>
    /// Tên file Excel được upload (VI)
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian upload (EN)<br/>
    /// Thời gian upload (VI)
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Danh sách transaction data được extract từ Excel (EN)<br/>
    /// Danh sách transaction data được extract từ Excel (VI)
    /// </summary>
    public List<TransactionDataRow> TransactionData { get; set; } = new();
}

/// <summary>
/// Represents a single row of transaction data from Excel file (EN)<br/>
/// Đại diện cho một dòng dữ liệu giao dịch từ file Excel (VI)
/// </summary>
public class TransactionDataRow
{
    /// <summary>
    /// The transaction date (EN)<br/>
    /// Ngày giao dịch (VI)
    /// </summary>
    public DateTime? TransactionDate { get; set; }

    /// <summary>
    /// Description or memo of the transaction (EN)<br/>
    /// Mô tả hoặc ghi chú của giao dịch (VI)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Transaction amount (EN)<br/>
    /// Số tiền giao dịch (VI)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Reference or ID of the transaction (EN)<br/>
    /// Mã tham chiếu hoặc ID của giao dịch (VI)
    /// </summary>
    public string Reference { get; set; } = string.Empty;    /// <summary>
    /// Raw data from Excel row for debugging purposes (EN)<br/>
    /// Dữ liệu thô từ dòng Excel cho mục đích debug (VI)
    /// </summary>
    public Dictionary<string, string> RawData { get; set; } = new();
}

/// <summary>
/// Response message after processing transaction data (EN)<br/>
/// Response message sau khi xử lý transaction data (VI)
/// </summary>
public class TransactionProcessedMessage
{
    /// <summary>
    /// Correlation ID để tracking request (EN)<br/>
    /// Correlation ID để tracking request (VI)
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    /// Indicates if processing was successful (EN)<br/>
    /// Cho biết việc xử lý có thành công hay không (VI)
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of transactions processed successfully (EN)<br/>
    /// Số lượng giao dịch được xử lý thành công (VI)
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Number of transactions failed to process (EN)<br/>
    /// Số lượng giao dịch xử lý thất bại (VI)
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// List of errors occurred during processing (EN)<br/>
    /// Danh sách lỗi xảy ra trong quá trình xử lý (VI)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Processing completion time (EN)<br/>
    /// Thời gian hoàn thành xử lý (VI)
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}
