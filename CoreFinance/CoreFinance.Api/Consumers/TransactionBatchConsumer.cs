using System.Diagnostics;
using CoreFinance.Contracts.Messages;
using MassTransit;
using SerilogContext = Serilog.Context;

namespace CoreFinance.Api.Consumers;

/// <summary>
/// Consumer for handling transaction batch messages from ExcelApi
/// Consumer để xử lý transaction batch messages từ ExcelApi
/// </summary>
public class TransactionBatchConsumer(
    ILogger<TransactionBatchConsumer> logger, 
    ActivitySource activitySource)
    : IConsumer<TransactionBatchInitiated>
{
    /// <summary>
    /// Process transaction batch initiated message from ExcelApi
    /// Xử lý transaction batch message từ ExcelApi
    /// </summary>
    public async Task Consume(ConsumeContext<TransactionBatchInitiated> context)
    {
        var message = context.Message;
        var startTime = DateTime.UtcNow;

        // Create OpenTelemetry span for message processing
        // Tạo OpenTelemetry span cho xử lý message
        using var activity = activitySource.StartActivity($"RabbitMQ receive {nameof(TransactionBatchInitiated)}");
        
        if (activity != null)
        {
            // Set message processing tags
            // Đặt tags cho xử lý message
            activity.SetTag("messaging.system", "rabbitmq");
            activity.SetTag("messaging.operation", "receive");
            activity.SetTag("messaging.destination", nameof(TransactionBatchInitiated));
            activity.SetTag("messaging.message_id", context.MessageId?.ToString());
            activity.SetTag("correlation.id", message.CorrelationId.ToString());
            activity.SetTag("batch.id", message.BatchId.ToString());
            activity.SetTag("batch.transaction_count", message.Transactions.Count);
            activity.SetTag("message.source", "ExcelApi");
        }

        // Add correlation context for distributed tracing
        using (SerilogContext.LogContext.PushProperty("CorrelationId", message.CorrelationId))
        using (SerilogContext.LogContext.PushProperty("BatchId", message.BatchId))
        using (SerilogContext.LogContext.PushProperty("MessageType", nameof(TransactionBatchInitiated)))
        using (SerilogContext.LogContext.PushProperty("ConsumerType", nameof(TransactionBatchConsumer)))
        using (SerilogContext.LogContext.PushProperty("SourceService", "ExcelApi"))
        {
            logger.LogInformation(
                "TransactionBatch received from ExcelApi - CorrelationId: {CorrelationId}, BatchId: {BatchId}, FileName: {FileName}, TransactionCount: {TransactionCount}, Source: {Source}, ReceivedAt: {ReceivedAt}",
                message.CorrelationId, message.BatchId, message.FileName, message.TransactionCount, message.Source, startTime);

            try
            {
                // Process the transaction batch
                var result = await ProcessTransactionBatch(message, context);

                var duration = DateTime.UtcNow - startTime;
                
                // Update activity with success metrics
                // Cập nhật activity với success metrics
                activity?.SetTag("batch.processed_count", result.ProcessedCount);
                activity?.SetTag("batch.failed_count", result.FailedCount);
                activity?.SetTag("batch.duration_ms", duration.TotalMilliseconds);
                
                if (result.FailedCount > 0)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, $"{result.FailedCount} transactions failed");
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                
                logger.LogInformation(
                    "TransactionBatch processing completed - CorrelationId: {CorrelationId}, BatchId: {BatchId}, ProcessedCount: {ProcessedCount}, FailedCount: {FailedCount}, Duration: {DurationMs}ms",
                    message.CorrelationId, message.BatchId, result.ProcessedCount, result.FailedCount, duration.TotalMilliseconds);

                if (result.FailedCount > 0)
                    logger.LogWarning(
                        "Some transactions failed in batch processing - CorrelationId: {CorrelationId}, BatchId: {BatchId}, Errors: {@Errors}",
                        message.CorrelationId, message.BatchId, result.Errors);

                // Publish completion message back to distributed tracing
                await PublishBatchCompletionMessage(message, result, context);
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                
                // Record exception in activity
                // Ghi lại exception trong activity
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.SetTag("error", true);
                activity?.SetTag("error.type", ex.GetType().FullName);
                activity?.SetTag("error.message", ex.Message);
                
                logger.LogError(ex,
                    "Critical error processing TransactionBatch - CorrelationId: {CorrelationId}, BatchId: {BatchId}, FileName: {FileName}, Duration: {DurationMs}ms, Error: {ErrorMessage}",
                    message.CorrelationId, message.BatchId, message.FileName, duration.TotalMilliseconds, ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Process transaction batch with individual transaction handling
    /// Xử lý transaction batch với xử lý từng transaction
    /// </summary>
    private async Task<BatchProcessingResult> ProcessTransactionBatch(
        TransactionBatchInitiated message,
        ConsumeContext<TransactionBatchInitiated> context)
    {
        var processedCount = 0;
        var failedCount = 0;
        var errors = new List<string>();

        logger.LogDebug("Starting batch processing - CorrelationId: {CorrelationId}, BatchId: {BatchId}, TransactionCount: {TransactionCount}",
            message.CorrelationId, message.BatchId, message.Transactions.Count);

        foreach (var transaction in message.Transactions)
        {
            try
            {
                // Process individual transaction from ExcelApi
                await ProcessIndividualTransaction(transaction, message);
                processedCount++;

                logger.LogTrace(
                    "Transaction processed from ExcelApi - TransactionId: {TransactionId}, Description: {Description}, Amount: {Amount}, CorrelationId: {CorrelationId}, BatchId: {BatchId}",
                    transaction.TransactionId, transaction.Description, transaction.Amount, message.CorrelationId, message.BatchId);
            }
            catch (Exception ex)
            {
                failedCount++;
                var error = $"Failed to process transaction {transaction.TransactionId}: {ex.Message}";
                errors.Add(error);

                logger.LogError(ex,
                    "Transaction processing failed from ExcelApi - TransactionId: {TransactionId}, Description: {Description}, CorrelationId: {CorrelationId}, BatchId: {BatchId}, Error: {ErrorMessage}",
                    transaction.TransactionId, transaction.Description, message.CorrelationId, message.BatchId, ex.Message);
            }
        }

        return new BatchProcessingResult
        {
            ProcessedCount = processedCount,
            FailedCount = failedCount,
            Errors = errors
        };
    }

    /// <summary>
    /// Process individual transaction from Excel batch
    /// Xử lý từng transaction từ Excel batch
    /// </summary>
    private async Task ProcessIndividualTransaction(TransactionData transaction, TransactionBatchInitiated batchMessage)
    {
        // Validate transaction data from Excel
        if (string.IsNullOrWhiteSpace(transaction.Description))
            throw new ArgumentException("Transaction description is required");

        if (transaction.Amount == 0)
            throw new ArgumentException("Transaction amount cannot be zero");

        if (transaction.TransactionDate == default)
            throw new ArgumentException("Transaction date is required");

        // Simulate processing delay (in real system, this would be database operations)
        await Task.Delay(Random.Shared.Next(5, 15)); // 5-15ms processing time

        // Log successful processing with rich context
        logger.LogDebug(
            "Successfully validated and processed transaction from ExcelApi - TransactionId: {TransactionId}, Description: {Description}, Amount: {Amount}, Category: {Category}, Source: ExcelApi, CorrelationId: {CorrelationId}",
            transaction.TransactionId, transaction.Description, transaction.Amount, transaction.Category, batchMessage.CorrelationId);

        // In real implementation, this would:
        // 1. Save transaction to CoreFinance database
        // 2. Update account balances
        // 3. Apply business rules specific to CoreFinance
        // 4. Trigger other domain events
        // 5. Send response back to ExcelApi if needed
    }    /// <summary>
    /// Publish batch completion message for distributed tracing
    /// Publish batch completion message cho distributed tracing
    /// </summary>
    private async Task PublishBatchCompletionMessage(
        TransactionBatchInitiated originalMessage,
        BatchProcessingResult result,
        ConsumeContext<TransactionBatchInitiated> context)
    {
        var completionMessage = new TransactionBatchCompleted
        {
            BatchId = originalMessage.BatchId,
            CorrelationId = originalMessage.CorrelationId,
            Source = "CoreFinance",
            CompletedAt = DateTime.UtcNow,
            ProcessedCount = result.ProcessedCount,
            FailedCount = result.FailedCount,
            Errors = result.Errors,
            ProcessingDuration = DateTime.UtcNow - originalMessage.OccurredAt
        };

        await context.Publish(completionMessage);

        logger.LogInformation(
            "Published TransactionBatchCompleted message - CorrelationId: {CorrelationId}, BatchId: {BatchId}, Success: {Success}",
            completionMessage.CorrelationId, completionMessage.BatchId, completionMessage.FailedCount == 0);
    }
}
