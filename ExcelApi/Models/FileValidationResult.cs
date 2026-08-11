namespace ExcelApi.Models;

/// <summary>
///     Validation result for file upload (EN)<br />
///     Kết quả validation cho file upload (VI)
/// </summary>
public class FileValidationResult
{
    /// <summary>
    ///     Indicates if the file is valid (EN)<br />
    ///     Cho biết file có hợp lệ hay không (VI)
    /// </summary>
    public bool IsValid { get; set; } = true;

    /// <summary>
    ///     List of validation errors (EN)<br />
    ///     Danh sách các lỗi validation (VI)
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    ///     List of validation warnings (EN)<br />
    ///     Danh sách các cảnh báo validation (VI)
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    ///     File information (EN)<br />
    ///     Thông tin file (VI)
    /// </summary>
    public FileInfo? FileInfo { get; set; }

    /// <summary>
    ///     Adds an error to the validation result (EN)<br />
    ///     Thêm lỗi vào kết quả validation (VI)
    /// </summary>
    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }

    /// <summary>
    ///     Adds a warning to the validation result (EN)<br />
    ///     Thêm cảnh báo vào kết quả validation (VI)
    /// </summary>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

/// <summary>
///     Information about the uploaded file (EN)<br />
///     Thông tin về file được upload (VI)
/// </summary>
public class FileInfo
{
    /// <summary>
    ///     Original file name (EN)<br />
    ///     Tên file gốc (VI)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    ///     File size in bytes (EN)<br />
    ///     Kích thước file tính bằng byte (VI)
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    ///     File content type (EN)<br />
    ///     Loại nội dung file (VI)
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    ///     File extension (EN)<br />
    ///     Phần mở rộng file (VI)
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    ///     Whether the file is password protected (EN)<br />
    ///     File có được bảo vệ bằng mật khẩu hay không (VI)
    /// </summary>
    public bool IsPasswordProtected { get; set; }

    /// <summary>
    ///     Upload timestamp (EN)<br />
    ///     Thời gian upload (VI)
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}