using System.ComponentModel.DataAnnotations;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.DTOs.SharedExpense;

/// <summary>
///     Request model for updating a shared expense (EN)<br />
///     Model yêu cầu để cập nhật chi tiêu chung (VI)
/// </summary>
public class UpdateSharedExpenseRequestDto
{
    /// <summary>
    ///     Shared expense ID (EN)<br />
    ///     ID chi tiêu chung (VI)
    /// </summary>
    [Required(ErrorMessage = "Expense ID is required")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Expense title (EN)<br />
    ///     Tiêu đề chi tiêu (VI)
    /// </summary>
    [Required(ErrorMessage = "Expense title is required")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Expense description (EN)<br />
    ///     Mô tả chi tiêu (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    /// <summary>
    ///     Total expense amount (EN)<br />
    ///     Tổng số tiền chi tiêu (VI)
    /// </summary>
    [Required(ErrorMessage = "Total amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Expense date (EN)<br />
    ///     Ngày chi tiêu (VI)
    /// </summary>
    [Required(ErrorMessage = "Expense date is required")]
    public DateTime ExpenseDate { get; set; }

    /// <summary>
    ///     Expense category (EN)<br />
    ///     Danh mục chi tiêu (VI)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    /// <summary>
    ///     Expense status (EN)<br />
    ///     Trạng thái chi tiêu (VI)
    /// </summary>
    public SharedExpenseStatus? Status { get; set; }

    /// <summary>
    ///     Group name or identifier (EN)<br />
    ///     Tên nhóm hoặc định danh (VI)
    /// </summary>
    [MaxLength(200, ErrorMessage = "Group name cannot exceed 200 characters")]
    public string? GroupName { get; set; }

    /// <summary>
    ///     Currency code (EN)<br />
    ///     Mã tiền tệ (VI)
    /// </summary>
    [MaxLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency code must be 3 uppercase letters")]
    public string? CurrencyCode { get; set; }

    /// <summary>
    ///     Receipt or proof image URL (EN)<br />
    ///     URL hình ảnh hóa đơn hoặc bằng chứng (VI)
    /// </summary>
    [MaxLength(500, ErrorMessage = "Receipt image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Receipt image URL must be a valid URL")]
    public string? ReceiptImageUrl { get; set; }

    /// <summary>
    ///     Additional notes (EN)<br />
    ///     Ghi chú bổ sung (VI)
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}