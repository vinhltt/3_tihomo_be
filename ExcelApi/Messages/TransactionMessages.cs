using ExcelApi.Models;

namespace ExcelApi.Messages;

/// <summary>
///     Message được publish khi ExcelApi hoàn thành extract transaction data
/// </summary>
public class TransactionBatchInitiated
{
    public Guid BatchId { get; set; }
    public Guid CorrelationId { get; set; }
    public string Source { get; set; } = "ExcelApi";
    public DateTime OccurredAt { get; set; }
    public int TransactionCount { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<TransactionData> Transactions { get; set; } = new();
}

/// <summary>
///     Individual transaction từ Excel file
/// </summary>
public class TransactionData
{
    public Guid TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Dictionary<string, string> RawData { get; set; } = new();
}

/// <summary>
///     Message cho individual transaction processing
/// </summary>
public class ProcessTransactionCommand
{
    public Guid TransactionId { get; set; }
    public Guid BatchId { get; set; }
    public Guid CorrelationId { get; set; }
    public TransactionData Transaction { get; set; } = new();
    public DateTime RequestedAt { get; set; }
    public string Priority { get; set; } = "Normal"; // Low, Normal, High
}

/// <summary>
///     Health check message cho message queue monitoring
/// </summary>
public class MessageQueueHealthCheck
{
    public Guid CheckId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

/// <summary>
///     Message được publish khi ExcelApi hoàn thành extract transaction data từ Excel file
/// </summary>
public class UploadTransactionDataMessage
{
    /// <summary>
    ///     Correlation ID để tracking request (EN)<br />
    ///     Correlation ID để tracking request (VI)
    /// </summary>
    public Guid CorrelationId { get; set; }

    /// <summary>
    ///     Tên file Excel được upload (EN)<br />
    ///     Tên file Excel được upload (VI)
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    ///     Thời gian upload (EN)<br />
    ///     Thời gian upload (VI)
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    ///     Danh sách transaction data được extract từ Excel (EN)<br />
    ///     Danh sách transaction data được extract từ Excel (VI)
    /// </summary>
    public List<TransactionDataRow> TransactionData { get; set; } = new();
}