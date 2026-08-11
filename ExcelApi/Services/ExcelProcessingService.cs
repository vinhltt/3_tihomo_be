using System.Data;
using ExcelApi.Messages;
using ExcelApi.Models;
using ExcelDataReader;
using MassTransit;

namespace ExcelApi.Services;

/// <summary>
///     Service for processing Excel files (XLSX format only)
///     Service để xử lý Excel files (chỉ hỗ trợ XLSX format)
/// </summary>
public class ExcelProcessingService(
    ILogger<ExcelProcessingService> logger,
    IPublishEndpoint publishEndpoint,
    LocalCorrelationContextService correlationContext)
    : IExcelProcessingService
{
    private const string XLSX_EXTENSION = ".xlsx";
    private const int DEFAULT_HEADER_ROW_INDEX = 24; // Excel row 25 (0-based index)
    private const string DEFAULT_END_MARKER = "Total Debit Transaction";

    /// <inheritdoc />
    public async Task<List<Dictionary<string, string>>> ProcessExcelFileAsync(
        IFormFile file,
        string? password,
        List<string>? headers = null,
        int? headerRowIndex = null,
        string? endMarker = null)
    {
        var correlationId = correlationContext.CorrelationId;
        logger.LogInformation(
            "Starting Excel file processing - CorrelationId: {CorrelationId}, FileName: {FileName}, FileSize: {FileSize}",
            correlationId, file.FileName, file.Length);

        try
        {
            // Verify file extension is XLSX
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != XLSX_EXTENSION)
            {
                logger.LogError(
                    "Invalid file format - CorrelationId: {CorrelationId}, FileName: {FileName}, Extension: {Extension}",
                    correlationId, file.FileName, extension);
                throw new InvalidOperationException(
                    "Only XLSX format is supported. Please convert your Excel file to XLSX format.");
            }

            logger.LogDebug(
                "File validation passed - CorrelationId: {CorrelationId}, HeaderRowIndex: {HeaderRowIndex}, EndMarker: {EndMarker}",
                correlationId, headerRowIndex ?? DEFAULT_HEADER_ROW_INDEX,
                endMarker ?? DEFAULT_END_MARKER); // Process Excel file with ExcelDataReader
            var extractedData = await Task.Run(() => ProcessExcelFileWithDataReader(
                file,
                password,
                headers,
                headerRowIndex ?? DEFAULT_HEADER_ROW_INDEX,
                endMarker ?? DEFAULT_END_MARKER));

            logger.LogInformation(
                "Excel processing completed - CorrelationId: {CorrelationId}, RecordsExtracted: {RecordCount}",
                correlationId, extractedData.Count);

            // Publish message to message queue for further processing
            await PublishTransactionDataAsync(file.FileName, extractedData);

            logger.LogInformation(
                "Excel file processing completed successfully - CorrelationId: {CorrelationId}, FileName: {FileName}",
                correlationId, file.FileName);

            return extractedData;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing Excel file - CorrelationId: {CorrelationId}, FileName: {FileName}, Error: {ErrorMessage}",
                correlationId, file.FileName, ex.Message);
            throw;
        }
    }

    /// <summary>
    ///     Publishes transaction data to message queue for processing by CoreFinance
    ///     Publish dữ liệu transaction lên message queue để CoreFinance xử lý
    /// </summary>
    private async Task PublishTransactionDataAsync(string fileName, List<Dictionary<string, string>> extractedData)
    {
        var correlationId = correlationContext.CorrelationId;

        try
        {
            logger.LogInformation(
                "Preparing transaction data for message publishing - CorrelationId: {CorrelationId}, FileName: {FileName}, RecordCount: {RecordCount}",
                correlationId, fileName, extractedData.Count);

            var transactionData = ConvertToTransactionDataRows(extractedData);

            var message = new UploadTransactionDataMessage
            {
                CorrelationId = correlationId,
                FileName = fileName,
                UploadedAt = DateTime.UtcNow,
                TransactionData = transactionData
            };

            logger.LogDebug(
                "Publishing message to RabbitMQ - CorrelationId: {CorrelationId}, MessageType: {MessageType}",
                correlationId, nameof(UploadTransactionDataMessage));

            await publishEndpoint.Publish(message);

            logger.LogInformation(
                "Message published successfully - CorrelationId: {CorrelationId}, FileName: {FileName}, TransactionCount: {Count}, PublishedAt: {PublishedAt}",
                correlationId, fileName, transactionData.Count, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish transaction data message - CorrelationId: {CorrelationId}, FileName: {FileName}, Error: {ErrorMessage}",
                correlationId, fileName, ex.Message);
            throw;
        }
    }

    /// <summary>
    ///     Converts raw Excel data to structured transaction data rows
    ///     Chuyển đổi dữ liệu Excel thô thành structured transaction data rows
    /// </summary>
    private static List<TransactionDataRow> ConvertToTransactionDataRows(List<Dictionary<string, string>> extractedData)
    {
        var transactionData = new List<TransactionDataRow>();

        foreach (var row in extractedData)
        {
            var transactionRow = new TransactionDataRow
            {
                TransactionDate = ParseDateFromRow(row),
                Description = GetValueFromRow(row, "Description", "Memo", "Detail"),
                Amount = ParseAmountFromRow(row),
                Reference = GetValueFromRow(row, "Reference", "Ref", "ID"),
                RawData = row
            };

            transactionData.Add(transactionRow);
        }

        return transactionData;
    }

    /// <summary>
    ///     Attempts to parse date from various possible column names
    ///     Cố gắng parse date từ các tên cột có thể có
    /// </summary>
    private static DateTime? ParseDateFromRow(Dictionary<string, string> row)
    {
        var dateColumns = new[] { "Date", "Transaction Date", "Trans Date", "TxnDate", "TDate" };

        foreach (var column in dateColumns)
            if (row.TryGetValue(column, out var dateValue) && !string.IsNullOrWhiteSpace(dateValue))
                if (DateTime.TryParse(dateValue, out var parsedDate))
                    return parsedDate;

        return null;
    }

    /// <summary>
    ///     Attempts to parse amount from various possible column names
    ///     Cố gắng parse amount từ các tên cột có thể có
    /// </summary>
    private static decimal ParseAmountFromRow(Dictionary<string, string> row)
    {
        var amountColumns = new[] { "Amount", "Value", "Credit", "Debit", "Money", "Sum" };

        foreach (var column in amountColumns)
            if (row.TryGetValue(column, out var amountValue) && !string.IsNullOrWhiteSpace(amountValue))
            {
                // Remove common currency symbols and formatting
                var cleanAmount = amountValue.Replace("$", "").Replace(",", "").Replace("(", "-").Replace(")", "")
                    .Trim();

                if (decimal.TryParse(cleanAmount, out var parsedAmount)) return parsedAmount;
            }

        return 0m;
    }

    /// <summary>
    ///     Gets value from row by trying multiple possible column names
    ///     Lấy value từ row bằng cách thử nhiều tên cột có thể có
    /// </summary>
    private static string GetValueFromRow(Dictionary<string, string> row, params string[] possibleColumns)
    {
        foreach (var column in possibleColumns)
            if (row.TryGetValue(column, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;

        return string.Empty;
    }

    private List<Dictionary<string, string>> ProcessExcelFileWithDataReader(
        IFormFile file,
        string? password,
        List<string>? clientHeaders,
        int headerRowIndex,
        string endMarker)
    {
        using var stream = file.OpenReadStream();
        using var reader = CreateExcelReader(stream, password);

        // Convert Excel data to DataSet
        var result = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = false // Don't use the first row as headers since we'll handle it manually
            }
        });

        // Get the first worksheet
        if (result.Tables.Count == 0) throw new InvalidOperationException("Excel file doesn't contain any worksheets");

        var dataTable = result.Tables[0]; // Get headers (either from client or from Excel file)
        List<string> headers;
        if (clientHeaders is { Count: > 0 })
        {
            logger.LogInformation("Using client-provided headers instead of reading from Excel");
            headers = clientHeaders;
        }
        else
        {
            logger.LogInformation("Reading headers from row {HeaderRowNumber} (index {HeaderRowIndex})",
                headerRowIndex + 1, headerRowIndex);

            // Check if header row exists
            if (dataTable.Rows.Count <= headerRowIndex)
                throw new InvalidOperationException(
                    $"Header row not found at row {headerRowIndex + 1} (index {headerRowIndex})");

            headers = ExtractHeaders(dataTable.Rows[headerRowIndex]);
        }

        return ExtractData(dataTable, headers, headerRowIndex, endMarker);
    }

    private static IExcelDataReader CreateExcelReader(Stream stream, string? password)
    {
        var configuration = new ExcelReaderConfiguration
        {
            Password = password
        };

        return ExcelReaderFactory.CreateReader(stream, configuration);
    }

    private static List<string> ExtractHeaders(DataRow headerRow)
    {
        var headers = new List<string>();

        for (var i = 0; i < headerRow.ItemArray.Length; i++)
        {
            var value = headerRow[i].ToString() ?? string.Empty;
            headers.Add(value);
        }

        return headers;
    }

    private List<Dictionary<string, string>> ExtractData(
        DataTable dataTable,
        List<string> headers,
        int headerRowIndex,
        string endMarker)
    {
        var data = new List<Dictionary<string, string>>();

        // Start from the row after the header
        for (var rowIndex = headerRowIndex + 1; rowIndex < dataTable.Rows.Count; rowIndex++)
        {
            var row = dataTable.Rows[rowIndex];
            var rowData = new Dictionary<string, string>(); // Check if this cell contains the end marker text
            if (row[0].ToString()?.Contains(endMarker) ?? false)
            {
                logger.LogInformation("Found end marker '{EndMarker}' at row {RowNumber}. Stopping processing.",
                    endMarker, rowIndex + 1);
                break;
            }

            for (var colIndex = 0; colIndex < headers.Count && colIndex < row.ItemArray.Length; colIndex++)
            {
                var cellValue = row[colIndex].ToString() ?? string.Empty;

                rowData[headers[colIndex]] = cellValue;
            }

            data.Add(rowData);
        }

        return data;
    }
}