namespace MoneyManagement.Application.DTOs.Jar;

/// <summary>
///     Result model for jar transfer operations (EN)<br />
///     Model kết quả cho các thao tác chuyển tiền giữa lọ (VI)
/// </summary>
public class TransferResultDto
{
    /// <summary>
    ///     Transaction ID for this transfer (EN)<br />
    ///     ID giao dịch cho việc chuyển tiền này (VI)
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    ///     Source jar information (EN)<br />
    ///     Thông tin lọ nguồn (VI)
    /// </summary>
    public JarResponseDto FromJar { get; set; } = null!;

    /// <summary>
    ///     Destination jar information (EN)<br />
    ///     Thông tin lọ đích (VI)
    /// </summary>
    public JarResponseDto ToJar { get; set; } = null!;

    /// <summary>
    ///     Amount transferred (EN)<br />
    ///     Số tiền đã chuyển (VI)
    /// </summary>
    public decimal AmountTransferred { get; set; }

    /// <summary>
    ///     Transfer description (EN)<br />
    ///     Mô tả việc chuyển tiền (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Transfer date and time (EN)<br />
    ///     Ngày giờ chuyển tiền (VI)
    /// </summary>
    public DateTime TransferredAt { get; set; }

    /// <summary>
    ///     Whether the transfer was successful (EN)<br />
    ///     Việc chuyển tiền có thành công không (VI)
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    ///     Error message if transfer failed (EN)<br />
    ///     Thông báo lỗi nếu chuyển tiền thất bại (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }
}