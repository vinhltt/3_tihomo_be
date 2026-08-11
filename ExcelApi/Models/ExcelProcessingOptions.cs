namespace ExcelApi.Models;

/// <summary>
///     Configuration options for Excel processing (EN)<br />
///     Các tùy chọn cấu hình cho việc xử lý Excel (VI)
/// </summary>
public class ExcelProcessingOptions
{
    /// <summary>
    ///     Maximum file size allowed in bytes (EN)<br />
    ///     Kích thước file tối đa cho phép tính bằng byte (VI)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB default

    /// <summary>
    ///     Allowed file extensions (EN)<br />
    ///     Các phần mở rộng file được phép (VI)
    /// </summary>
    public HashSet<string> AllowedExtensions { get; set; } = new() { ".xlsx", ".xls" };

    /// <summary>
    ///     Default header row index if not specified (EN)<br />
    ///     Chỉ số dòng header mặc định nếu không được chỉ định (VI)
    /// </summary>
    public int DefaultHeaderRowIndex { get; set; } = 24; // Row 25 in Excel (0-based)

    /// <summary>
    ///     Default end marker text (EN)<br />
    ///     Văn bản dấu hiệu kết thúc mặc định (VI)
    /// </summary>
    public string DefaultEndMarker { get; set; } = "Total Debit Transaction";

    /// <summary>
    ///     Maximum number of rows to process (EN)<br />
    ///     Số dòng tối đa để xử lý (VI)
    /// </summary>
    public int MaxRowsToProcess { get; set; } = 10000;

    /// <summary>
    ///     Whether to skip empty rows (EN)<br />
    ///     Có bỏ qua các dòng trống hay không (VI)
    /// </summary>
    public bool SkipEmptyRows { get; set; } = true;

    /// <summary>
    ///     Whether to trim whitespace from cell values (EN)<br />
    ///     Có loại bỏ khoảng trắng từ giá trị ô hay không (VI)
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    ///     Whether to validate header names (EN)<br />
    ///     Có validate tên header hay không (VI)
    /// </summary>
    public bool ValidateHeaders { get; set; } = true;

    /// <summary>
    ///     Timeout for processing in seconds (EN)<br />
    ///     Timeout cho việc xử lý tính bằng giây (VI)
    /// </summary>
    public int ProcessingTimeoutSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    ///     Whether to include row numbers in output (EN)<br />
    ///     Có bao gồm số dòng trong output hay không (VI)
    /// </summary>
    public bool IncludeRowNumbers { get; set; } = true;

    /// <summary>
    ///     Whether to validate data types (EN)<br />
    ///     Có validate kiểu dữ liệu hay không (VI)
    /// </summary>
    public bool ValidateDataTypes { get; set; } = false;

    /// <summary>
    ///     Custom date format patterns to try when parsing dates (EN)<br />
    ///     Các pattern định dạng ngày tùy chỉnh để thử khi parse ngày (VI)
    /// </summary>
    public List<string> DateFormats { get; set; } = new()
    {
        "dd/MM/yyyy",
        "MM/dd/yyyy",
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "yyyy/MM/dd"
    };

    /// <summary>
    ///     Culture info for parsing numbers and dates (EN)<br />
    ///     Thông tin culture để parse số và ngày (VI)
    /// </summary>
    public string CultureInfo { get; set; } = "en-US";
}

/// <summary>
///     Column mapping configuration for Excel processing (EN)<br />
///     Cấu hình mapping cột cho việc xử lý Excel (VI)
/// </summary>
public class ColumnMapping
{
    /// <summary>
    ///     Excel column name (EN)<br />
    ///     Tên cột Excel (VI)
    /// </summary>
    public string ExcelColumnName { get; set; } = string.Empty;

    /// <summary>
    ///     Output property name (EN)<br />
    ///     Tên thuộc tính output (VI)
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    ///     Data type for validation (EN)<br />
    ///     Kiểu dữ liệu để validation (VI)
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    ///     Whether this column is required (EN)<br />
    ///     Cột này có bắt buộc hay không (VI)
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    ///     Default value if cell is empty (EN)<br />
    ///     Giá trị mặc định nếu ô trống (VI)
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    ///     Custom validation pattern (regex) (EN)<br />
    ///     Pattern validation tùy chỉnh (regex) (VI)
    /// </summary>
    public string? ValidationPattern { get; set; }

    /// <summary>
    ///     Error message for validation failures (EN)<br />
    ///     Thông báo lỗi cho validation thất bại (VI)
    /// </summary>
    public string? ValidationErrorMessage { get; set; }
}