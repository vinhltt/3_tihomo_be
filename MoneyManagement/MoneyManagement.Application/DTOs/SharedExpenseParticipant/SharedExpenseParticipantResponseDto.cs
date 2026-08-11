namespace MoneyManagement.Application.DTOs.SharedExpenseParticipant;

/// <summary>
///     Response model for shared expense participant data (EN)<br />
///     Model phản hồi cho dữ liệu người tham gia chi tiêu chung (VI)
/// </summary>
public class SharedExpenseParticipantResponseDto
{
    /// <summary>
    ///     Participant ID (EN)<br />
    ///     ID người tham gia (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Shared expense ID (EN)<br />
    ///     ID chi tiêu chung (VI)
    /// </summary>
    public Guid? SharedExpenseId { get; set; }

    /// <summary>
    ///     User ID (for registered users) (EN)<br />
    ///     ID người dùng (cho người dùng đã đăng ký) (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Participant name (for non-users) (EN)<br />
    ///     Tên người tham gia (cho người không phải user) (VI)
    /// </summary>
    public string? ParticipantName { get; set; }

    /// <summary>
    ///     Participant email (EN)<br />
    ///     Email người tham gia (VI)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    ///     Participant phone number (EN)<br />
    ///     Số điện thoại người tham gia (VI)
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Share amount for this participant (EN)<br />
    ///     Số tiền chia sẻ cho người tham gia này (VI)
    /// </summary>
    public decimal ShareAmount { get; set; }

    /// <summary>
    ///     Amount already paid by this participant (EN)<br />
    ///     Số tiền người tham gia này đã trả (VI)
    /// </summary>
    public decimal PaidAmount { get; set; }

    /// <summary>
    ///     Remaining amount this participant owes (EN)<br />
    ///     Số tiền còn lại người tham gia này nợ (VI)
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    ///     Whether this participant has fully paid (EN)<br />
    ///     Người tham gia này đã trả đầy đủ chưa (VI)
    /// </summary>
    public bool IsSettled { get; set; }

    /// <summary>
    ///     Date when participant settled their share (EN)<br />
    ///     Ngày người tham gia thanh toán phần của họ (VI)
    /// </summary>
    public DateTime? SettledDate { get; set; }

    /// <summary>
    ///     Payment method used (EN)<br />
    ///     Phương thức thanh toán đã sử dụng (VI)
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    ///     Additional notes for this participant (EN)<br />
    ///     Ghi chú bổ sung cho người tham gia này (VI)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    ///     Created date (EN)<br />
    ///     Ngày tạo (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Updated date (EN)<br />
    ///     Ngày cập nhật (VI)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Get payment percentage (EN)<br />
    ///     Lấy phần trăm thanh toán (VI)
    /// </summary>
    public decimal PaymentPercentage { get; set; }
}