namespace ExcelApi.Models;

/// <summary>
///     Model for file upload request
/// </summary>
public class FileUploadModel
{
    /// <summary>
    ///     Excel file to be processed
    /// </summary>
    public IFormFile? File { get; set; }

    /// <summary>
    ///     Optional password for protected Excel files
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    ///     Optional list of headers to use instead of reading from the Excel file
    ///     If provided, these headers will be used instead of reading from row 25
    /// </summary>
    public List<string>? Headers { get; set; }

    /// <summary>
    ///     Optional index of the header row (0-based index)
    ///     Default is 24 (row 25 in Excel)
    /// </summary>
    public int? HeaderRowIndex { get; set; }

    /// <summary>
    ///     Optional marker text that indicates where to stop processing
    ///     Default is "Total Debit Transaction"
    /// </summary>
    public string? EndMarker { get; set; }
}