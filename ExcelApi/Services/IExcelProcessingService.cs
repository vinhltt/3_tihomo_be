namespace ExcelApi.Services;

/// <summary>
///     Service interface for processing Excel files
/// </summary>
public interface IExcelProcessingService
{
    /// <summary>
    ///     Process an Excel file and extract data
    /// </summary>
    /// <param name="file">The Excel file to process</param>
    /// <param name="password">Optional password for protected files</param>
    /// <param name="headers">Optional list of headers to use instead of reading from the Excel file</param>
    /// <param name="headerRowIndex">Optional index of the header row (0-based)</param>
    /// <param name="endMarker">Optional text marker to indicate where to stop processing</param>
    /// <returns>Dictionary containing the processed data</returns>
    Task<List<Dictionary<string, string>>> ProcessExcelFileAsync(
        IFormFile file,
        string? password,
        List<string>? headers = null,
        int? headerRowIndex = null,
        string? endMarker = null);
}