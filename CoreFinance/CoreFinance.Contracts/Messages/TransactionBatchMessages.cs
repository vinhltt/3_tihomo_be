namespace CoreFinance.Contracts.Messages
{
    /// <summary>
    /// Message được publish khi ExcelApi hoàn thành extract transaction data
    /// Message published when ExcelApi completes transaction data extraction
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
    /// Individual transaction từ Excel file
    /// Individual transaction from Excel file
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
    /// Message cho batch processing completion
    /// Message for batch processing completion
    /// </summary>
    public class TransactionBatchCompleted
    {
        public Guid BatchId { get; set; }
        public Guid CorrelationId { get; set; }
        public string Source { get; set; } = "CoreFinance";
        public DateTime CompletedAt { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public TimeSpan ProcessingDuration { get; set; }
    }

    /// <summary>
    /// Result của batch processing
    /// Result of batch processing
    /// </summary>
    public class BatchProcessingResult
    {
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<ProcessedTransaction> ProcessedTransactions { get; set; } = new();
    }

    /// <summary>
    /// Individual processed transaction result
    /// </summary>
    public class ProcessedTransaction
    {
        public Guid TransactionId { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
