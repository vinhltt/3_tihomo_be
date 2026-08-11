using ExcelApi.Models;
using ExcelApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ExcelApi.Controllers;

/// <summary>
///     Controller for handling Excel file operations
/// </summary>
/// <remarks>
///     Initializes a new instance of the ExcelController
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class ExcelController(IExcelProcessingService excelProcessingService) : ControllerBase
{
    /// <summary>
    ///     Uploads and extract data in Excel file
    /// </summary>
    /// <param name="model">The file upload model containing the Excel file and configuration options</param>
    /// <returns>Processed data from the Excel file</returns>
    /// <response code="200">Returns the processed data</response>
    /// <response code="400">If the file is null or empty</response>
    /// <response code="500">If there was an internal error processing the file</response>
    [HttpPost("extract")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Extract data in Excel file",
        Description =
            "Uploads an Excel file and processes its content. Headers, header row index, and end marker can all be customized."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExtractDataInExcel([FromForm] FileUploadModel model)
    {
        // Handle the case where the file is passed directly in the model
        var file = model?.File;

        // If File property is null, check if any file was uploaded with any parameter name
        if (file == null && Request.Form.Files.Count > 0)
            // Get the first uploaded file regardless of parameter name
            file = Request.Form.Files[0];

        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        try
        {
            var result = await excelProcessingService.ProcessExcelFileAsync(
                file,
                model?.Password,
                model?.Headers,
                model?.HeaderRowIndex,
                model?.EndMarker);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing file: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles files sent with dynamic field names (for n8n integration)
    /// </summary>
    [HttpPost("extract-dynamic")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Extract data in Excel file with dynamic file naming",
        Description =
            "Alternative endpoint that supports dynamic file field names for integration with workflow tools like n8n."
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExtractDataWithDynamicFileField()
    {
        // Get form data
        var password = Request.Form["Password"].FirstOrDefault();
        List<string> headersText = Request.Form["Headers"].ToList()!;
        var headerRowIndexText = Request.Form["HeaderRowIndex"].FirstOrDefault();
        var endMarker = Request.Form["EndMarker"].FirstOrDefault();

        // Parse headers if provided
        List<string> headers = headersText;
        // Parse header row index if provided
        int? headerRowIndex = null;
        if (!string.IsNullOrEmpty(headerRowIndexText) && int.TryParse(headerRowIndexText, out var index))
            headerRowIndex = index;

        // Get the first file, regardless of field name
        if (Request.Form.Files.Count == 0) return BadRequest("No file uploaded.");

        var file = Request.Form.Files[0];
        if (file.Length == 0) return BadRequest("Empty file uploaded.");

        try
        {
            var result = await excelProcessingService.ProcessExcelFileAsync(
                file,
                password,
                headers,
                headerRowIndex,
                endMarker);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error processing file: {ex.Message}");
        }
    }

    /// <summary>
    /// Test endpoint để publish message và test distributed tracing
    /// </summary>
    /// <returns>Test result với correlation ID</returns>
    [HttpPost("test-distributed-tracing")]
    [SwaggerOperation(
        Summary = "Test distributed tracing",
        Description = "Publishes test message để test distributed tracing flow ExcelApi → CoreFinance",
        OperationId = "TestDistributedTracing"
    )]
    [SwaggerResponse(200, "Message published successfully", typeof(object))]
    [SwaggerResponse(500, "Internal server error")]
    public async Task<IActionResult> TestDistributedTracing(
        [FromServices] MessagePublishingService messageService,
        [FromServices] LocalCorrelationContextService correlationContext,
        [FromServices] ILogger<ExcelController> logger)
    {
        try
        {
            var correlationId = correlationContext.CorrelationId;
            
            logger.LogInformation(
                "=== DISTRIBUTED TRACING TEST START === CorrelationId: {CorrelationId}",
                correlationId);

            // Create sample transaction data
            var sampleData = new List<Dictionary<string, string>>
            {
                new() {
                    ["Date"] = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                    ["Description"] = "Test Payment from ExcelApi",
                    ["Amount"] = "150.75",
                    ["Category"] = "Testing",
                    ["Reference"] = "TRACE-001",
                    ["Account"] = "CHECKING"
                },
                new() {
                    ["Date"] = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["Description"] = "Test Deposit from ExcelApi", 
                    ["Amount"] = "200.00",
                    ["Category"] = "Income",
                    ["Reference"] = "TRACE-002",
                    ["Account"] = "CHECKING"
                }
            };

            logger.LogInformation(
                "Publishing TransactionBatchInitiated message - CorrelationId: {CorrelationId}, Records: {RecordCount}",
                correlationId, sampleData.Count);

            await messageService.PublishTransactionBatchAsync(sampleData, "distributed-tracing-test.xlsx");

            logger.LogInformation(
                "=== DISTRIBUTED TRACING TEST PUBLISHED === CorrelationId: {CorrelationId}, Records: {RecordCount}",
                correlationId, sampleData.Count);

            return Ok(new
            {
                success = true,
                message = "Distributed tracing test message published successfully",
                correlationId = correlationId,
                recordCount = sampleData.Count,
                timestamp = DateTime.UtcNow,
                serviceName = "ExcelApi",
                fileName = "distributed-tracing-test.xlsx",
                instruction = $"Check logs in Grafana with query: {{CorrelationId=\"{correlationId}\"}}"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish distributed tracing test message - Error: {ErrorMessage}",
                ex.Message);

            return StatusCode(500, new
            {
                success = false,
                message = "Failed to publish test message",
                error = ex.Message
            });
        }
    }
}