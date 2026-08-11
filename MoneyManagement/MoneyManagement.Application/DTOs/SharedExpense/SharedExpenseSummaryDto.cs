using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.SharedExpense;

/// <summary>
///     Summary model for shared expense data (EN)<br />
///     Model tóm tắt cho dữ liệu chi tiêu chung (VI)
/// </summary>
public class SharedExpenseSummaryDto
{
    /// <summary>
    ///     Shared expense ID (EN)<br />
    ///     ID chi tiêu chung (VI)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Shared expense ID (duplicate for compatibility) (EN)<br />
    ///     ID chi tiêu chung (bản sao để tương thích) (VI)
    /// </summary>
    public Guid SharedExpenseId { get; set; }

    /// <summary>
    ///     Expense title (EN)<br />
    ///     Tiêu đề chi tiêu (VI)
    /// </summary>
    public string Title { get; set; } = string.Empty;

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
    public string CurrencyCode { get; set; } = "VND";

    /// <summary>
    ///     Number of participants (EN)<br />
    ///     Số lượng người tham gia (VI)
    /// </summary>
    public int ParticipantCount { get; set; }

    /// <summary>
    ///     Number of participants who have paid (EN)<br />
    ///     Số lượng người tham gia đã thanh toán (VI)
    /// </summary>
    public int SettledParticipantCount { get; set; }

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

    /// <summary>
    ///     Created date (EN)<br />
    ///     Ngày tạo (VI)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Total amount paid by all participants (EN)<br />
    ///     Tổng số tiền đã trả bởi tất cả người tham gia (VI)
    /// </summary>
    public decimal TotalPaidAmount { get; set; }

    /// <summary>
    ///     Number of participants who have fully settled (EN)<br />
    ///     Số lượng người tham gia đã thanh toán đầy đủ (VI)
    /// </summary>
    public int SettledParticipantsCount { get; set; }

    /// <summary>
    ///     Number of participants who have not settled (EN)<br />
    ///     Số lượng người tham gia chưa thanh toán (VI)
    /// </summary>
    public int UnsettledParticipantsCount { get; set; }
}