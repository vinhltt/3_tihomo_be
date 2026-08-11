using System.ComponentModel.DataAnnotations;

namespace MoneyManagement.Application.DTOs.SharedExpenseParticipant;

/// <summary>
///     Request model for creating a shared expense participant (EN)<br />
///     Model yêu cầu để tạo người tham gia chi tiêu chung (VI)
/// </summary>
public class CreateSharedExpenseParticipantRequestDto
{
    /// <summary>
    ///     Shared expense ID that this participant belongs to (EN)<br />
    ///     ID chi tiêu chung mà người tham gia này thuộc về (VI)
    /// </summary>
    [Required(ErrorMessage = "Shared expense ID is required")]
    public Guid SharedExpenseId { get; set; }

    /// <summary>
    ///     User ID (for registered users) (EN)<br />
    ///     ID người dùng (cho người dùng đã đăng ký) (VI)
    /// </summary>
    public Guid? UserId { get; set; }

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
    [Required(ErrorMessage = "Share amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Share amount must be greater than 0")]
    public decimal ShareAmount { get; set; }

    /// <summary>
    ///     Additional notes for this participant (EN)<br />
    ///     Ghi chú bổ sung cho người tham gia này (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    /// <summary>
    ///     Validation method to ensure either UserId or ParticipantName is provided (EN)<br />
    ///     Phương thức xác thực để đảm bảo có UserId hoặc ParticipantName (VI)
    /// </summary>
    public bool IsValid()
    {
        return UserId.HasValue || !string.IsNullOrWhiteSpace(ParticipantName);
    }
}