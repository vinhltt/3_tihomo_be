using MoneyManagement.Application.DTOs.SharedExpenseParticipant;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.SharedExpense;

/// <summary>
///     Response model for shared expense data (EN)<br />
///     Model phản hồi cho dữ liệu chi tiêu chung (VI)
/// </summary>
public class SharedExpenseResponseDto
{
    /// <summary>
    ///     Shared expense ID (EN)<br />
    ///     ID chi tiêu chung (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Foreign key linking to user who created the expense (EN)<br />
    ///     Khóa ngoại liên kết với người dùng tạo chi tiêu (VI)
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    ///     Expense title (EN)<br />
    ///     Tiêu đề chi tiêu (VI)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Expense description (EN)<br />
    ///     Mô tả chi tiêu (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Total expense amount (EN)<br />
    ///     Tổng số tiền chi tiêu (VI)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Amount already settled (EN)<br />
    ///     Số tiền đã thanh toán (VI)
    /// </summary>
    public decimal SettledAmount { get; set; }

    /// <summary>
    ///     Remaining amount to be settled (EN)<br />
    ///     Số tiền còn lại cần thanh toán (VI)
    /// </summary>
    public decimal RemainingAmount { get; set; }

    /// <summary>
    ///     Expense date (EN)<br />
    ///     Ngày chi tiêu (VI)
    /// </summary>
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    ///     Expense category (EN)<br />
    ///     Danh mục chi tiêu (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     Expense status (EN)<br />
    ///     Trạng thái chi tiêu (VI)
    /// </summary>
    public SharedExpenseStatus Status { get; set; }

    /// <summary>
    ///     Group name or identifier (EN)<br />
    ///     Tên nhóm hoặc định danh (VI)
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    ///     Currency code (EN)<br />
    ///     Mã tiền tệ (VI)
    /// </summary>
    public string? CurrencyCode { get; set; } = "VND";

    /// <summary>
    ///     Receipt or proof image URL (EN)<br />
    ///     URL hình ảnh hóa đơn hoặc bằng chứng (VI)
    /// </summary>
    public string? ReceiptImageUrl { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
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
    ///     List of participants in this shared expense (EN)<br />
    ///     Danh sách người tham gia trong chi tiêu chung này (VI)
    /// </summary>
    public List<SharedExpenseParticipantResponseDto> Participants { get; set; } = [];

    /// <summary>
    ///     Check if expense is fully settled (EN)<br />
    ///     Kiểm tra chi tiêu đã thanh toán đầy đủ chưa (VI)
    /// </summary>
    public bool IsFullySettled { get; set; }

    /// <summary>
    ///     Get settlement percentage (EN)<br />
    ///     Lấy phần trăm thanh toán (VI)
    /// </summary>
    public decimal SettlementPercentage { get; set; }
}