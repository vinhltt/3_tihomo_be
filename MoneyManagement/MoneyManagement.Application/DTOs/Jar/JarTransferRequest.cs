using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Request model for transferring money between jars (EN)<br />
///     Model yêu cầu để chuyển tiền giữa các lọ (VI)
/// </summary>
public class JarTransferRequest
{
    /// <summary>
    ///     Source jar identifier (EN)<br />
    ///     Định danh lọ nguồn (VI)
    /// </summary>
    [Required(ErrorMessage = "Source jar ID is required")]
    public Guid FromJarId { get; set; }

    /// <summary>
    ///     Destination jar identifier (EN)<br />
    ///     Định danh lọ đích (VI)
    /// </summary>
    [Required(ErrorMessage = "Destination jar ID is required")]
    public Guid ToJarId { get; set; }

    /// <summary>
    ///     Transfer amount (EN)<br />
    ///     Số tiền chuyển (VI)
    /// </summary>
    [Required(ErrorMessage = "Transfer amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Transfer amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    ///     Transfer description (EN)<br />
    ///     Mô tả chuyển tiền (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

/// <summary>
///     Result model for jar transfer operation (EN)<br />
///     Model kết quả cho thao tác chuyển tiền giữa lọ (VI)
/// </summary>
public class JarTransferResult
{
    /// <summary>
    ///     Source jar after transfer (EN)<br />
    ///     Lọ nguồn sau khi chuyển (VI)
    /// </summary>
    public JarViewModel FromJar { get; set; } = new();

    /// <summary>
    ///     Destination jar after transfer (EN)<br />
    ///     Lọ đích sau khi chuyển (VI)
    /// </summary>
    public JarViewModel ToJar { get; set; } = new();

    /// <summary>
    ///     Transfer amount (EN)<br />
    ///     Số tiền đã chuyển (VI)
    /// </summary>
    public decimal TransferAmount { get; set; }

    /// <summary>
    ///     Success status (EN)<br />
    ///     Trạng thái thành công (VI)
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    ///     Result message (EN)<br />
    ///     Thông báo kết quả (VI)
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    ///     Transfer timestamp (EN)<br />
    ///     Thời gian chuyển tiền (VI)
    /// </summary>
    public DateTime TransferTime { get; set; }
}