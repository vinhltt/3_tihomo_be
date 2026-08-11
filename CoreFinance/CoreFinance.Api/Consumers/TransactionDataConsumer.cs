using CoreFinance.Contracts.Messages;
using MassTransit;
using SerilogContext = Serilog.Context;

namespace CoreFinance.Api.Consumers;

/// <summary>
///     Consumer for handling transaction data uploaded from Excel files
///     Consumer để xử lý dữ liệu transaction upload từ Excel files
/// </summary>
public class TransactionDataConsumer(ILogger<TransactionDataConsumer> logger)
    : IConsumer<UploadTransactionDataMessage>
{
    /// <summary>
    ///     Process uploaded transaction data from Excel file
    ///     Xử lý dữ liệu transaction upload từ Excel file
    /// </summary>
    public async Task Consume(ConsumeContext<UploadTransactionDataMessage> context)
    {
        var message = context.Message;
        var startTime = DateTime.UtcNow;

        // Add correlation context for all logs in this consumer
        using (SerilogContext.LogContext.PushProperty("CorrelationId", message.CorrelationId))
        using (SerilogContext.LogContext.PushProperty("FileName", message.FileName))
        using (SerilogContext.LogContext.PushProperty("MessageType", nameof(UploadTransactionDataMessage)))
        using (SerilogContext.LogContext.PushProperty("ConsumerType", nameof(TransactionDataConsumer)))
        {
            logger.LogInformation(
                "Message received - CorrelationId: {CorrelationId}, FileName: {FileName}, TransactionCount: {Count}, UploadedAt: {UploadedAt}, ReceivedAt: {ReceivedAt}",
                message.CorrelationId, message.FileName, message.TransactionData.Count, message.UploadedAt, startTime);

            try
            {
                // Process transactions with detailed metrics
                var result = await ProcessTransactionBatch(message, context);

                var duration = DateTime.UtcNow - startTime;
                logger.LogInformation(
                    "Message processing completed - CorrelationId: {CorrelationId}, ProcessedCount: {ProcessedCount}, FailedCount: {FailedCount}, Duration: {DurationMs}ms",
                    message.CorrelationId, result.ProcessedCount, result.FailedCount, duration.TotalMilliseconds);
                if (result.FailedCount > 0)
                    logger.LogWarning(
                        "Some transactions failed processing - CorrelationId: {CorrelationId}, Errors: {@Errors}",
                        message.CorrelationId, result.Errors);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                logger.LogError(ex,
                    "Critical error processing message - CorrelationId: {CorrelationId}, FileName: {FileName}, Duration: {DurationMs}ms, Error: {ErrorMessage}",
                    message.CorrelationId, message.FileName, duration.TotalMilliseconds, ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    ///     Process batch of transactions with metrics tracking
    ///     Xử lý batch transactions với metrics tracking
    /// </summary>
    private async Task<ProcessingResult> ProcessTransactionBatch(UploadTransactionDataMessage message,
        ConsumeContext<UploadTransactionDataMessage> context)
    {
        var processedCount = 0;
        var failedCount = 0;
        var errors = new List<string>();
        logger.LogDebug("Starting batch processing - CorrelationId: {CorrelationId}, BatchSize: {BatchSize}",
            message.CorrelationId, message.TransactionData.Count);

        foreach (var transactionRow in message.TransactionData)
            try
            {
                // Stub processing logic - validate and process transaction
                await ProcessTransactionRow(transactionRow, message.CorrelationId);
                processedCount++;

                logger.LogTrace(
                    "Transaction processed - Description: {Description}, Amount: {Amount}, Date: {Date}, CorrelationId: {CorrelationId}",
                    transactionRow.Description, transactionRow.Amount, transactionRow.TransactionDate,
                    message.CorrelationId);
            }
            catch (Exception ex)
            {
                failedCount++;
                var error = $"Failed to process transaction {transactionRow.Description}: {ex.Message}";
                errors.Add(error);

                logger.LogError(ex,
                    "Transaction processing failed - Description: {Description}, CorrelationId: {CorrelationId}, Error: {ErrorMessage}",
                    transactionRow.Description, message.CorrelationId, ex.Message);
            }

        // Send response message
        var responseMessage = new TransactionProcessedMessage
        {
            CorrelationId = message.CorrelationId,
            Success = failedCount == 0,
            ProcessedCount = processedCount,
            FailedCount = failedCount,
            Errors = errors,
            ProcessedAt = DateTime.UtcNow
        };

        await context.Publish(responseMessage);

        return new ProcessingResult
        {
            ProcessedCount = processedCount,
            FailedCount = failedCount,
            Errors = errors
        };
    }

    /// <summary>
    ///     Process individual transaction row (stub implementation)
    ///     Xử lý từng row transaction (stub implementation)
    /// </summary>
    private async Task ProcessTransactionRow(TransactionDataRow row, Guid correlationId)
    {
        // Stub implementation - simulate validation and processing

        // Validate transaction data
        if (string.IsNullOrWhiteSpace(row.Description))
            throw new ArgumentException("Transaction description is required");

        if (row.Amount == 0) throw new ArgumentException("Transaction amount cannot be zero");

        if (!row.TransactionDate.HasValue) throw new ArgumentException("Transaction date is required");

        // Simulate database operation delay
        await Task.Delay(10);
        // 10ms simulated processing time
        // Log successful processing
        logger.LogDebug(
            "Successfully validated and processed transaction {Description} with amount {Amount} for correlation ID {CorrelationId}",
            row.Description, row.Amount, correlationId); // In real implementation, this would:
        // 1. Save transaction to database
        // 2. Update account balances
        // 3. Apply business rules
        // 4. Trigger other domain events
    }
}

/// <summary>
///     Result of processing a batch of transactions
///     Kết quả xử lý batch transactions
/// </summary>
public class ProcessingResult
{
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}