using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Enums;

namespace CoreFinance.Application.DTOs.RecurringTransactionTemplate;

/// <summary>
///     Represents a request to update an existing recurring transaction template. (EN)<br />
///     Đại diện cho request cập nhật mẫu giao dịch định kỳ hiện có. (VI)
/// </summary>
public class RecurringTransactionTemplateUpdateRequest : BaseUpdateRequest<Guid>
{
    /// <summary>
    ///     The updated name of the template (optional). (EN)<br />
    ///     Tên của mẫu được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The updated description of the template (optional). (EN)<br />
    ///     Mô tả về mẫu được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     The updated amount of the recurring transaction (optional). (EN)<br />
    ///     Số tiền của giao dịch định kỳ được cập nhật (tùy chọn). (VI)
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    ///     The updated type of recurring transaction (optional). (EN)<br />
    ///     Loại giao dịch định kỳ được cập nhật (tùy chọn). (VI)
    /// </summary>
    public RecurringTransactionType? TransactionType { get; set; }

    /// <summary>
    ///     The updated category of the recurring transaction (optional). (EN)<br />
    ///     Danh mục của giao dịch định kỳ được cập nhật (tùy chọn). (VI)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    ///     The updated frequency of the recurrence (optional). (EN)<br />
    ///     Tần suất lặp lại được cập nhật (tùy chọn). (VI)
    /// </summary>
    public RecurrenceFrequency? Frequency { get; set; }

    /// <summary>
    ///     The updated custom interval in days for recurring transactions with custom frequency (optional). (EN)<br />
    ///     Khoảng thời gian tùy chỉnh theo ngày được cập nhật cho giao dịch định kỳ với tần suất tùy chỉnh (tùy chọn). (VI)
    /// </summary>
    public int? CustomIntervalDays { get; set; }

    /// <summary>
    ///     The updated start date for the recurring transaction (optional). (EN)<br />
    ///     Ngày bắt đầu được cập nhật cho giao dịch định kỳ (tùy chọn). (VI)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    ///     The updated end date for the recurring transaction (optional). (EN)<br />
    ///     Ngày kết thúc được cập nhật cho giao dịch định kỳ (tùy chọn). (VI)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    ///     The updated Cron expression for complex recurrence patterns (optional). (EN)<br />
    ///     Cron expression được cập nhật cho các mẫu lặp lại phức tạp (tùy chọn). (VI)
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    ///     The updated active status of the template (optional). (EN)<br />
    ///     Trạng thái hoạt động được cập nhật của mẫu (tùy chọn). (VI)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    ///     Indicates if expected transactions should be automatically generated from this template (optional). (EN)<br />
    ///     Cho biết giao dịch dự kiến có nên được tự động sinh ra từ mẫu này hay không (tùy chọn). (VI)
    /// </summary>
    public bool? AutoGenerate { get; set; }

    /// <summary>
    ///     The updated number of days in advance to generate expected transactions for (optional). (EN)<br />
    ///     Số ngày tạo giao dịch dự kiến trước được cập nhật (tùy chọn). (VI)
    /// </summary>
    public int? DaysInAdvance { get; set; }

    /// <summary>
    ///     Additional updated notes about the template (optional). (EN)<br />
    ///     Ghi chú bổ sung được cập nhật về mẫu (tùy chọn). (VI)
    /// </summary>
    public string? Notes { get; set; }
}