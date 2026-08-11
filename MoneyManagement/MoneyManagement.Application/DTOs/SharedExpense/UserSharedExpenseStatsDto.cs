namespace MoneyManagement.Application.DTOs.SharedExpense;

/// <summary>
///     Statistics model for user's shared expenses (EN)<br />
///     Model thống kê cho chi tiêu chung của người dùng (VI)
/// </summary>
public class UserSharedExpenseStatsDto
{
    /// <summary>
    ///     User ID (EN)<br />
    ///     ID người dùng (VI)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     Total number of expenses (EN)<br />
    ///     Tổng số chi tiêu (VI)
    /// </summary>
    public int TotalExpenses { get; set; }

    /// <summary>
    ///     Total number of shared expenses user has created (EN)<br />
    ///     Tổng số chi tiêu chung người dùng đã tạo (VI)
    /// </summary>
    public int TotalExpensesCreated { get; set; }

    /// <summary>
    ///     Total number of shared expenses user participates in (EN)<br />
    ///     Tổng số chi tiêu chung người dùng tham gia (VI)
    /// </summary>
    public int TotalExpensesParticipating { get; set; }

    /// <summary>
    ///     Total amount across all expenses (EN)<br />
    ///     Tổng số tiền trong tất cả chi tiêu (VI)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Total amount user owes (EN)<br />
    ///     Tổng số tiền người dùng nợ (VI)
    /// </summary>
    public decimal TotalOwedAmount { get; set; }

    /// <summary>
    ///     Total amount user has paid (EN)<br />
    ///     Tổng số tiền người dùng đã trả (VI)
    /// </summary>
    public decimal TotalPaidAmount { get; set; }

    /// <summary>
    ///     Total amount others owe to user (EN)<br />
    ///     Tổng số tiền người khác nợ người dùng (VI)
    /// </summary>
    public decimal TotalAmountOwedToUser { get; set; }

    /// <summary>
    ///     Number of pending expenses (EN)<br />
    ///     Số chi tiêu đang chờ xử lý (VI)
    /// </summary>
    public int PendingExpenses { get; set; }

    /// <summary>
    ///     Number of settled expenses (EN)<br />
    ///     Số chi tiêu đã thanh toán (VI)
    /// </summary>
    public int SettledExpenses { get; set; }

    /// <summary>
    ///     Number of partially settled expenses (EN)<br />
    ///     Số chi tiêu đã thanh toán một phần (VI)
    /// </summary>
    public int PartiallySettledExpenses { get; set; }

    /// <summary>
    ///     Number of user participations (EN)<br />
    ///     Số lần tham gia của người dùng (VI)
    /// </summary>
    public int UserParticipations { get; set; }

    /// <summary>
    ///     Total amount user owes to others (EN)<br />
    ///     Tổng số tiền người dùng nợ người khác (VI)
    /// </summary>
    public decimal UserTotalOwed { get; set; }

    /// <summary>
    ///     Total amount user has paid to others (EN)<br />
    ///     Tổng số tiền người dùng đã trả cho người khác (VI)
    /// </summary>
    public decimal UserTotalPaid { get; set; }

    /// <summary>
    ///     Total outstanding amount for user (EN)<br />
    ///     Tổng số tiền còn nợ của người dùng (VI)
    /// </summary>
    public decimal UserTotalOutstanding { get; set; }

    /// <summary>
    ///     Average expense amount (EN)<br />
    ///     Số tiền chi tiêu trung bình (VI)
    /// </summary>
    public decimal AverageExpenseAmount { get; set; }

    /// <summary>
    ///     Most frequent expense category (EN)<br />
    ///     Danh mục chi tiêu thường xuyên nhất (VI)
    /// </summary>
    public string? MostFrequentCategory { get; set; }

    /// <summary>
    ///     Settlement rate as percentage (EN)<br />
    ///     Tỷ lệ thanh toán theo phần trăm (VI)
    /// </summary>
    public decimal SettlementRate { get; set; }

    /// <summary>
    ///     Last expense date (EN)<br />
    ///     Ngày chi tiêu gần nhất (VI)
    /// </summary>
    public DateTime? LastExpenseDate { get; set; }

    /// <summary>
    ///     Currency used most frequently (EN)<br />
    ///     Tiền tệ được sử dụng thường xuyên nhất (VI)
    /// </summary>
    public string? PreferredCurrency { get; set; }
}