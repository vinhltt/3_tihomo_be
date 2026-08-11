namespace ExcelApi.Models;

/// <summary>
///     Represents the result of Excel file processing (EN)<br />
///     Đại diện cho kết quả xử lý file Excel (VI)
/// </summary>
public class ExcelProcessingResult
{
    /// <summary>
    ///     Indicates if the processing was successful (EN)<br />
    ///     Cho biết việc xử lý có thành công hay không (VI)
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    ///     Error message if processing failed (EN)<br />
    ///     Thông báo lỗi nếu xử lý thất bại (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     List of processed data rows from Excel (EN)<br />
    ///     Danh sách các dòng dữ liệu đã xử lý từ Excel (VI)
    /// </summary>
    public List<ExcelRowData> Data { get; set; } = new();

    /// <summary>
    ///     Metadata about the processing (EN)<br />
    ///     Metadata về quá trình xử lý (VI)
    /// </summary>
    public ExcelProcessingMetadata Metadata { get; set; } = new();
}

/// <summary>
///     Represents a single row of data from Excel (EN)<br />
///     Đại diện cho một dòng dữ liệu từ Excel (VI)
/// </summary>
public class ExcelRowData
{
    /// <summary>
    ///     Row number in the Excel file (1-based) (EN)<br />
    ///     Số dòng trong file Excel (bắt đầu từ 1) (VI)
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    ///     Dictionary containing column name and value pairs (EN)<br />
    ///     Dictionary chứa các cặp tên cột và giá trị (VI)
    /// </summary>
    public Dictionary<string, string> Values { get; set; } = new();

    /// <summary>
    ///     Indicates if this row has any validation errors (EN)<br />
    ///     Cho biết dòng này có lỗi validation hay không (VI)
    /// </summary>
    public bool HasErrors { get; set; }

    /// <summary>
    ///     List of validation errors for this row (EN)<br />
    ///     Danh sách các lỗi validation cho dòng này (VI)
    /// </summary>
    public List<string> Errors { get; set; } = new();
}

/// <summary>
///     Metadata about the Excel processing operation (EN)<br />
///     Metadata về quá trình xử lý Excel (VI)
/// </summary>
public class ExcelProcessingMetadata
{
    /// <summary>
    ///     Name of the processed file (EN)<br />
    ///     Tên file đã được xử lý (VI)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     Size of the file in bytes (EN)<br />
    ///     Kích thước file tính bằng byte (VI)
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    ///     Total number of rows processed (EN)<br />
    ///     Tổng số dòng đã được xử lý (VI)
    /// </summary>
    public int TotalRowsProcessed { get; set; }

    /// <summary>
    ///     Number of rows with data (EN)<br />
    ///     Số dòng có dữ liệu (VI)
    /// </summary>
    public int DataRowsCount { get; set; }

    /// <summary>
    ///     Number of rows with errors (EN)<br />
    ///     Số dòng có lỗi (VI)
    /// </summary>
    public int ErrorRowsCount { get; set; }

    /// <summary>
    ///     List of headers used for processing (EN)<br />
    ///     Danh sách headers được sử dụng để xử lý (VI)
    /// </summary>
    public List<string> Headers { get; set; } = new();

    /// <summary>
    ///     Index of the header row used (0-based) (EN)<br />
    ///     Chỉ số dòng header được sử dụng (bắt đầu từ 0) (VI)
    /// </summary>
    public int HeaderRowIndex { get; set; }

    /// <summary>
    ///     End marker used to stop processing (EN)<br />
    ///     Dấu hiệu kết thúc được sử dụng để dừng xử lý (VI)
    /// </summary>
    public string? EndMarker { get; set; }

    /// <summary>
    ///     Processing timestamp (EN)<br />
    ///     Thời gian xử lý (VI)
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Processing duration in milliseconds (EN)<br />
    ///     Thời gian xử lý tính bằng milliseconds (VI)
    /// </summary>
    public long ProcessingDurationMs { get; set; }
}