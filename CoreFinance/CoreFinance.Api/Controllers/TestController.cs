using CoreFinance.Contracts.Messages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace CoreFinance.Api.Controllers;

/// <summary>
/// Test controller for testing RabbitMQ and logging integration
/// Controller test cho tích hợp RabbitMQ và logging
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController(IPublishEndpoint publishEndpoint, ILogger<TestController> logger) : ControllerBase
{
    /// <summary>
    /// Test endpoint to publish a sample message to RabbitMQ for testing the full flow
    /// Endpoint test để publish message mẫu vào RabbitMQ để test full flow
    /// </summary>
    [HttpPost("publish-test-message")]
    public async Task<IActionResult> PublishTestMessage()
    {
        var correlationId = Guid.CreateVersion7();
        var fileName = $"test-file-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";

        logger.LogInformation("Publishing test message - CorrelationId: {CorrelationId}, FileName: {FileName}",
            correlationId, fileName);

        // Create a test message with sample transaction data
        var testMessage = new UploadTransactionDataMessage
        {
            CorrelationId = correlationId,
            FileName = fileName,
            UploadedAt = DateTime.UtcNow,
            TransactionData =
            [                
                new()
                {
                    Description = "Test Transaction 1 - Coffee Shop",
                    Amount = -25.50m,
                    TransactionDate = DateTime.Today.AddDays(-1),
                    Reference = "TEST001",
                    RawData = new Dictionary<string, string>
                    {
                        { "Original_Description", "COFFEE SHOP PURCHASE" },
                        { "Location", "DOWNTOWN" }
                    }
                },
                new()
                {
                    Description = "Test Transaction 2 - Salary Deposit",
                    Amount = 3500.00m,
                    TransactionDate = DateTime.Today.AddDays(-3),
                    Reference = "TEST002",
                    RawData = new Dictionary<string, string>
                    {
                        { "Original_Description", "SALARY DIRECT DEPOSIT" },
                        { "Employer", "TECH COMPANY INC" }
                    }
                },
                new()
                {
                    Description = "Test Transaction 3 - Grocery Store",
                    Amount = -127.89m,
                    TransactionDate = DateTime.Today.AddDays(-2),
                    Reference = "TEST003",
                    RawData = new Dictionary<string, string>
                    {
                        { "Original_Description", "SUPERMARKET PURCHASE" },
                        { "Location", "MAIN STREET" }
                    }
                }
            ]
        };

        try
        {
            // Publish the message to RabbitMQ
            await publishEndpoint.Publish(testMessage);

            logger.LogInformation("Test message published successfully - CorrelationId: {CorrelationId}, TransactionCount: {TransactionCount}",
                correlationId, testMessage.TransactionData.Count);

            return Ok(new
            {
                success = true,
                correlationId,
                fileName,
                transactionCount = testMessage.TransactionData.Count,
                message = "Test message published successfully. Check RabbitMQ management UI and Grafana logs."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish test message - CorrelationId: {CorrelationId}, Error: {ErrorMessage}",
                correlationId, ex.Message);

            return StatusCode(500, new
            {
                success = false,
                correlationId,
                error = ex.Message,
                message = "Failed to publish test message"
            });
        }
    }

    /// <summary>
    /// Test endpoint to generate sample logs for testing Loki integration
    /// Endpoint test để tạo log mẫu cho việc test tích hợp Loki
    /// </summary>
    [HttpPost("generate-test-logs")]
    public IActionResult GenerateTestLogs()
    {
        var correlationId = Guid.CreateVersion7();

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("MessageType", "TestLogMessage"))
        using (Serilog.Context.LogContext.PushProperty("ConsumerType", "TestController"))
        {
            logger.LogTrace("This is a TRACE level log message for testing");
            logger.LogDebug("This is a DEBUG level log message for testing");
            logger.LogInformation("This is an INFO level log message for testing - CorrelationId: {CorrelationId}", correlationId);
            logger.LogWarning("This is a WARNING level log message for testing - CorrelationId: {CorrelationId}", correlationId);

            try
            {
                // Simulate an error scenario
                throw new InvalidOperationException("This is a simulated error for testing purposes");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "This is an ERROR level log message for testing - CorrelationId: {CorrelationId}, Error: {ErrorMessage}",
                    correlationId, ex.Message);
            }
        }

        return Ok(new
        {
            success = true,
            correlationId,
            message = "Test logs generated successfully. Check console output, log files, and Grafana/Loki."
        });
    }

    /// <summary>
    /// Health check endpoint
    /// Endpoint health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "CoreFinance Test Controller"
        });
    }
}
