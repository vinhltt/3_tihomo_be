using MassTransit;
using ExcelApi.Messages;

namespace ExcelApi.Services
{
    /// <summary>
    /// Service for publishing messages với enhanced message processing
    /// </summary>
    public class MessagePublishingService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly LocalCorrelationContextService _correlationContext;
        private readonly ILogger<MessagePublishingService> _logger;        
        public MessagePublishingService(
            IPublishEndpoint publishEndpoint,
            LocalCorrelationContextService correlationContext,
            ILogger<MessagePublishingService> logger)
        {
            _publishEndpoint = publishEndpoint;
            _correlationContext = correlationContext;
            _logger = logger;
        }

        /// <summary>
        /// Publish transaction batch message sau khi extract từ Excel
        /// </summary>
        public async Task PublishTransactionBatchAsync(
            List<Dictionary<string, string>> extractedData,
            string fileName)
        {
            var correlationId = _correlationContext.CorrelationId;
            var batchId = Guid.CreateVersion7();

            // Convert extracted data to transaction format (stub logic for testing)
            var transactions = ConvertToTransactionData(extractedData);

            var message = new TransactionBatchInitiated
            {
                BatchId = batchId,
                CorrelationId = correlationId,
                Source = "ExcelApi",
                OccurredAt = DateTime.UtcNow,
                TransactionCount = transactions.Count,
                FileName = fileName,
                Transactions = transactions,
                Metadata = new Dictionary<string, object>
                {
                    ["processing_time"] = DateTime.UtcNow,
                    ["file_size"] = fileName.Length,
                    ["extraction_method"] = "excel_reader",
                    ["record_count"] = extractedData.Count
                }
            };

            _logger.LogInformation(
                "Publishing TransactionBatchInitiated message. BatchId: {BatchId}, TransactionCount: {TransactionCount}, CorrelationId: {CorrelationId}",
                batchId, transactions.Count, correlationId);

            await _publishEndpoint.Publish(message);            // TODO: Implement individual transaction processing if needed
            // Currently focusing on batch processing for distributed tracing
            
            /*
            // Publish individual transaction processing commands for parallel processing
            foreach (var transaction in transactions)
            {
                var command = new ProcessTransactionCommand
                {
                    TransactionId = transaction.TransactionId,
                    BatchId = batchId,
                    CorrelationId = correlationId,
                    Transaction = transaction,
                    RequestedAt = DateTime.UtcNow,
                    Priority = "Normal"
                };

                await _publishEndpoint.Publish(command);
            }

            _logger.LogInformation(
                "Published {CommandCount} ProcessTransactionCommand messages for batch {BatchId}",
                transactions.Count, batchId);
            */
        }

        /// <summary>
        /// Convert extracted Excel data to transaction format (stub implementation for testing)
        /// </summary>
        private List<TransactionData> ConvertToTransactionData(List<Dictionary<string, string>> extractedData)
        {
            var transactions = new List<TransactionData>();

            foreach (var row in extractedData.Take(10)) // Limit for testing
            {
                // Simple stub conversion logic - trong production sẽ có mapping phức tạp hơn
                var transaction = new TransactionData
                {
                    TransactionId = Guid.CreateVersion7(),
                    TransactionDate = TryParseDate(row.GetValueOrDefault("Date", "")) ?? DateTime.Today,
                    Amount = TryParseDecimal(row.GetValueOrDefault("Amount", "0")),
                    Description = row.GetValueOrDefault("Description", "Unknown Transaction"),
                    Category = DetermineCategory(row.GetValueOrDefault("Description", "")),
                    Reference = row.GetValueOrDefault("Reference", ""),
                    AccountType = "Bank", // Default for testing
                    RawData = row // Store original data for debugging
                };

                transactions.Add(transaction);
            }

            return transactions;
        }

        private DateTime? TryParseDate(string dateStr)
        {
            if (DateTime.TryParse(dateStr, out var date))
                return date;
            return null;
        }

        private decimal TryParseDecimal(string amountStr)
        {
            if (decimal.TryParse(amountStr, out var amount))
                return amount;
            return 0m;
        }

        private string DetermineCategory(string description)
        {
            // Simple category determination logic for testing
            var desc = description.ToLowerInvariant();
            if (desc.Contains("atm") || desc.Contains("withdraw"))
                return "Cash Withdrawal";
            if (desc.Contains("transfer"))
                return "Transfer";
            if (desc.Contains("payment"))
                return "Payment";
            return "Other";
        }
    }
}
