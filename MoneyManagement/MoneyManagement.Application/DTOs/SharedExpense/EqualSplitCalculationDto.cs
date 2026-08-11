namespace MoneyManagement.Application.DTOs.SharedExpense;

/// <summary>
///     Model for equal split calculation results (EN)<br />
///     Model cho kết quả tính toán chia đều (VI)
/// </summary>
public class EqualSplitCalculationDto
{
    /// <summary>
    ///     Shared expense ID (EN)<br />
    ///     ID chi phí chia sẻ (VI)
    /// </summary>
    public Guid SharedExpenseId { get; set; }

    /// <summary>
    ///     Total amount to be split (EN)<br />
    ///     Tổng số tiền cần chia (VI)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Total owed amount across all participants (EN)<br />
    ///     Tổng số tiền nợ của tất cả người tham gia (VI)
    /// </summary>
    public decimal OwedAmount { get; set; }

    /// <summary>
    ///     Number of participants (EN)<br />
    ///     Số lượng người tham gia (VI)
    /// </summary>
    public int ParticipantCount { get; set; }

    /// <summary>
    ///     List of participants in the split (EN)<br />
    ///     Danh sách người tham gia trong việc chia (VI)
    /// </summary>
    public List<ParticipantSplitDto> Participants { get; set; } = [];

    /// <summary>
    ///     Calculation details and notes (EN)<br />
    ///     Chi tiết tính toán và ghi chú (VI)
    /// </summary>
    public string? CalculationNotes { get; set; }
}

/// <summary>
///     Model for individual participant split calculation (EN)<br />
///     Model cho tính toán phân chia cá nhân của người tham gia (VI)
/// </summary>
public class ParticipantSplitDto
{
    /// <summary>
    ///     Participant ID (EN)<br />
    ///     ID người tham gia (VI)
    /// </summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>
    ///     User ID (for registered users) (EN)<br />
    ///     ID người dùng (cho người dùng đã đăng ký) (VI)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    ///     Participant name (EN)<br />
    ///     Tên người tham gia (VI)
    /// </summary>
    public string? ParticipantName { get; set; }

    /// <summary>
    ///     Calculated share amount (EN)<br />
    ///     Số tiền chia sẻ đã tính (VI)
    /// </summary>
    public decimal ShareAmount { get; set; }

    /// <summary>
    ///     Recommended share amount based on split calculation (EN)<br />
    ///     Số tiền chia sẻ được đề xuất dựa trên tính toán chia (VI)
    /// </summary>
    public decimal RecommendedShareAmount { get; set; }

    /// <summary>
    ///     Current share amount assigned to participant (EN)<br />
    ///     Số tiền chia sẻ hiện tại được gán cho người tham gia (VI)
    /// </summary>
    public decimal CurrentShareAmount { get; set; }

    /// <summary>
    ///     Difference between recommended and current share amounts (EN)<br />
    ///     Chênh lệch giữa số tiền chia sẻ được đề xuất và hiện tại (VI)
    /// </summary>
    public decimal Difference { get; set; }

    /// <summary>
    ///     Whether this participant gets the rounding adjustment (EN)<br />
    ///     Người tham gia này có nhận điều chỉnh làm tròn không (VI)
    /// </summary>
    public bool HasRoundingAdjustment { get; set; }

    /// <summary>
    ///     Rounding adjustment amount (if any) (EN)<br />
    ///     Số tiền điều chỉnh làm tròn (nếu có) (VI)
    /// </summary>
    public decimal RoundingAdjustment { get; set; }
}