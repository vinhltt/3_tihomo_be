using Microsoft.AspNetCore.Mvc;
using ExcelApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace ExcelApi.Controllers
{
    /// <summary>
    /// Controller for testing message queue integration với Excel processing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Message Queue Testing")]
    public class MessageTestController(
        IExcelProcessingService excelService,
        MessagePublishingService messageService,
        LocalCorrelationContextService correlationContext,
        ILogger<MessageTestController> logger) : ControllerBase
    {

        /// <summary>
        /// Test endpoint để upload Excel file và publish message queue
        /// </summary>
        /// <param name="file">Excel file to process and publish to message queue</param>
        /// <returns>Processing result với correlation ID</returns>
        [HttpPost("upload-and-publish")]
        [SwaggerOperation(
            Summary = "Upload Excel file and publish to message queue",
            Description = "Processes Excel file and publishes TransactionBatchInitiated message to RabbitMQ for CoreFinance consumption",
            OperationId = "UploadAndPublish"
        )]
        [SwaggerResponse(200, "File processed and messages published successfully")]
        [SwaggerResponse(400, "Invalid file or processing error")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> UploadAndPublishAsync(
            [SwaggerParameter("Excel file to process")]
            IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("File is required and cannot be empty");
                }

                var correlationId = correlationContext.CorrelationId;

                logger.LogInformation(
                    "Starting Excel file processing and message publishing. FileName: {FileName}, Size: {FileSize}, CorrelationId: {CorrelationId}",
                    file.FileName, file.Length, correlationId);

                // Step 1: Process Excel file
                var extractedData = await excelService.ProcessExcelFileAsync(file, null);

                logger.LogInformation(
                    "Excel processing completed. RecordCount: {RecordCount}, CorrelationId: {CorrelationId}",
                    extractedData.Count, correlationId);

                // Step 2: Publish messages to queue
                await messageService.PublishTransactionBatchAsync(extractedData, file.FileName);

                logger.LogInformation(
                    "Messages published successfully. CorrelationId: {CorrelationId}",
                    correlationId);

                return Ok(new
                {
                    success = true,
                    message = "File processed and messages published successfully",
                    correlationId = correlationId.ToString(),
                    fileName = file.FileName,
                    recordsProcessed = extractedData.Count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error processing file and publishing messages. FileName: {FileName}, CorrelationId: {CorrelationId}",
                    file.FileName, correlationContext.CorrelationId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error during processing",
                    error = ex.Message,
                    correlationId = correlationContext.CorrelationId.ToString()
                });
            }
        }

        /// <summary>
        /// Health check endpoint để test message queue connectivity
        /// </summary>
        /// <returns>Message queue health status</returns>
        [HttpGet("health")]
        [SwaggerOperation(
            Summary = "Check message queue health",
            Description = "Returns the health status of message queue connectivity",
            OperationId = "GetMessageQueueHealth"
        )]
        public IActionResult GetHealthAsync()
        {
            try
            {
                // Simple health check - trong production sẽ có logic phức tạp hơn
                var healthInfo = new
                {
                    status = "Healthy",
                    service = "ExcelApi-MessageQueue",
                    timestamp = DateTime.UtcNow,
                    correlationId = correlationContext.CorrelationId.ToString(),
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    version = "1.0.0"
                };

                return Ok(healthInfo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Health check failed for message queue");
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint để publish TransactionBatchInitiated message với sample data
        /// </summary>
        /// <returns>Message publishing result với correlation ID</returns>
        [HttpPost("test-batch-message")]
        [SwaggerOperation(
            Summary = "Test publish TransactionBatchInitiated message",
            Description = "Publishes test TransactionBatchInitiated message với sample data để test distributed tracing",
            OperationId = "TestBatchMessage"
        )]
        [SwaggerResponse(200, "Message published successfully", typeof(object))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> TestBatchMessage()
        {
            try
            {
                var correlationId = correlationContext.CorrelationId;
                
                logger.LogInformation(
                    "Starting test TransactionBatchInitiated message publish - CorrelationId: {CorrelationId}",
                    correlationId);

                // Create sample transaction data for testing
                var sampleData = new List<Dictionary<string, string>>
                {
                    new() {
                        ["Date"] = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                        ["Description"] = "Test Payment from ExcelApi",
                        ["Amount"] = "150.75",
                        ["Category"] = "Testing",
                        ["Reference"] = "TEST-001",
                        ["Account"] = "CHECKING"
                    },
                    new() {
                        ["Date"] = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"),
                        ["Description"] = "Test Transfer from ExcelApi",
                        ["Amount"] = "-75.25",
                        ["Category"] = "Transfer",
                        ["Reference"] = "TEST-002",
                        ["Account"] = "SAVINGS"
                    },
                    new() {
                        ["Date"] = DateTime.Now.ToString("yyyy-MM-dd"),
                        ["Description"] = "Test Deposit from ExcelApi",
                        ["Amount"] = "200.00",
                        ["Category"] = "Income",
                        ["Reference"] = "TEST-003",
                        ["Account"] = "CHECKING"
                    }
                };

                await messageService.PublishTransactionBatchAsync(sampleData, "test-transactions.xlsx");

                logger.LogInformation(
                    "Successfully published test TransactionBatchInitiated message - CorrelationId: {CorrelationId}, Records: {RecordCount}",
                    correlationId, sampleData.Count);

                return Ok(new
                {
                    success = true,
                    message = "Test TransactionBatchInitiated message published successfully",
                    correlationId,
                    recordCount = sampleData.Count,
                    timestamp = DateTime.UtcNow,
                    serviceName = "ExcelApi",
                    fileName = "test-transactions.xlsx"
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to publish test TransactionBatchInitiated message - CorrelationId: {CorrelationId}, Error: {ErrorMessage}",
                    correlationContext.CorrelationId, ex.Message);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to publish test message",
                    error = ex.Message,
                    correlationId = correlationContext.CorrelationId
                });
            }
        }
    }
}
