using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.SharedExpenseParticipant;

/// <summary>
///     Request model for updating a shared expense participant (EN)<br />
///     Model yêu cầu để cập nhật người tham gia chi tiêu chung (VI)
/// </summary>
public class UpdateSharedExpenseParticipantRequestDto
{
    /// <summary>
    ///     Participant ID (EN)<br />
    ///     ID người tham gia (VI)
    /// </summary>
    [Required(ErrorMessage = "Participant ID is required")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Participant name (for non-users) (EN)<br />
    ///     Tên người tham gia (cho người không phải user) (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Participant name cannot exceed 200 characters")]
    public string? ParticipantName { get; set; }

    /// <summary>
    ///     Participant email (EN)<br />
    ///     Email người tham gia (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    public string? Email { get; set; }

    /// <summary>
    ///     Participant phone number (EN)<br />
    ///     Số điện thoại người tham gia (VI)
    /// </summary>
    [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    ///     Share amount for this participant (EN)<br />
    ///     Số tiền chia sẻ cho người tham gia này (VI)
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "Share amount must be greater than 0")]
    public decimal? ShareAmount { get; set; }

    /// <summary>
    ///     Amount already paid by this participant (EN)<br />
    ///     Số tiền người tham gia này đã trả (VI)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be non-negative")]
    public decimal? PaidAmount { get; set; }

    /// <summary>
    ///     Whether this participant has fully paid (EN)<br />
    ///     Người tham gia này đã trả đầy đủ chưa (VI)
    /// </summary>
    public bool? IsSettled { get; set; }

    /// <summary>
    ///     Payment method used (EN)<br />
    ///     Phương thức thanh toán đã sử dụng (VI)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Payment method cannot exceed 100 characters")]
    public string? PaymentMethod { get; set; }

    /// <summary>
    ///     Additional notes for this participant (EN)<br />
    ///     Ghi chú bổ sung cho người tham gia này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}