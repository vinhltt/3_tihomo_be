using MoneyManagement.Application.DTOs.SharedExpense;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;

namespace MoneyManagement.Application.Interfaces;

/// <summary>
///     Service interface for managing shared expenses (EN)<br />
///     Interface dịch vụ quản lý chi tiêu chung (VI)
/// </summary>
public interface ISharedExpenseService
{
    #region SharedExpense Operations

    /// <summary>
    ///     Get all shared expenses for the current user (EN)<br />
    ///     Lấy tất cả chi tiêu chung của người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    Task<IEnumerable<SharedExpenseResponseDto>> GetAllSharedExpensesAsync();

    /// <summary>
    ///     Get shared expense by ID (EN)<br />
    ///     Lấy chi tiêu chung theo ID (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung (VI)</returns>
    Task<SharedExpenseResponseDto?> GetSharedExpenseByIdAsync(Guid id);

    /// <summary>
    ///     Create a new shared expense (EN)<br />
    ///     Tạo chi tiêu chung mới (VI)
    /// </summary>
    /// <param name="createDto">Create shared expense DTO (EN)<br />DTO tạo chi tiêu chung (VI)</param>
    /// <returns>Created shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã tạo (VI)</returns>
    Task<SharedExpenseResponseDto> CreateSharedExpenseAsync(CreateSharedExpenseRequestDto createDto);

    /// <summary>
    ///     Update an existing shared expense (EN)<br />
    ///     Cập nhật chi tiêu chung hiện có (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <param name="updateDto">Update shared expense DTO (EN)<br />DTO cập nhật chi tiêu chung (VI)</param>
    /// <returns>Updated shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã cập nhật (VI)</returns>
    Task<SharedExpenseResponseDto> UpdateSharedExpenseAsync(Guid id, UpdateSharedExpenseRequestDto updateDto);

    /// <summary>
    ///     Delete a shared expense (EN)<br />
    ///     Xóa chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Success indicator (EN)<br />Chỉ báo thành công (VI)</returns>
    Task<bool> DeleteSharedExpenseAsync(Guid id);

    /// <summary>
    ///     Get shared expenses by status (EN)<br />
    ///     Lấy chi tiêu chung theo trạng thái (VI)
    /// </summary>
    /// <param name="status">Shared expense status (EN)<br />Trạng thái chi tiêu chung (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByStatusAsync(int status);

    /// <summary>
    ///     Get shared expenses by date range (EN)<br />
    ///     Lấy chi tiêu chung theo khoảng thời gian (VI)
    /// </summary>
    /// <param name="startDate">Start date (EN)<br />Ngày bắt đầu (VI)</param>
    /// <param name="endDate">End date (EN)<br />Ngày kết thúc (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    ///     Get shared expenses by group name (EN)<br />
    ///     Lấy chi tiêu chung theo tên nhóm (VI)
    /// </summary>
    /// <param name="groupName">Group name (EN)<br />Tên nhóm (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    Task<IEnumerable<SharedExpenseResponseDto>> GetSharedExpensesByGroupAsync(string groupName);

    /// <summary>
    ///     Mark shared expense as settled (EN)<br />
    ///     Đánh dấu chi tiêu chung đã thanh toán (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Updated shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã cập nhật (VI)</returns>
    Task<SharedExpenseResponseDto> MarkAsSettledAsync(Guid id);

    #endregion

    #region SharedExpenseParticipant Operations

    /// <summary>
    ///     Get all participants for a shared expense (EN)<br />
    ///     Lấy tất cả người tham gia cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="sharedExpenseId">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>List of participant response DTOs (EN)<br />Danh sách DTO phản hồi người tham gia (VI)</returns>
    Task<IEnumerable<SharedExpenseParticipantResponseDto>> GetParticipantsBySharedExpenseIdAsync(Guid sharedExpenseId);

    /// <summary>
    ///     Add a participant to a shared expense (EN)<br />
    ///     Thêm người tham gia vào chi tiêu chung (VI)
    /// </summary>
    /// <param name="createDto">Create participant DTO (EN)<br />DTO tạo người tham gia (VI)</param>
    /// <returns>Created participant response DTO (EN)<br />DTO phản hồi người tham gia đã tạo (VI)</returns>
    Task<SharedExpenseParticipantResponseDto> AddParticipantAsync(CreateSharedExpenseParticipantRequestDto createDto);

    /// <summary>
    ///     Update a participant's information (EN)<br />
    ///     Cập nhật thông tin người tham gia (VI)
    /// </summary>
    /// <param name="id">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <param name="updateDto">Update participant DTO (EN)<br />DTO cập nhật người tham gia (VI)</param>
    /// <returns>Updated participant response DTO (EN)<br />DTO phản hồi người tham gia đã cập nhật (VI)</returns>
    Task<SharedExpenseParticipantResponseDto> UpdateParticipantAsync(Guid id,
        UpdateSharedExpenseParticipantRequestDto updateDto);

    /// <summary>
    ///     Remove a participant from a shared expense (EN)<br />
    ///     Xóa người tham gia khỏi chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <returns>Success indicator (EN)<br />Chỉ báo thành công (VI)</returns>
    Task<bool> RemoveParticipantAsync(Guid id);

    /// <summary>
    ///     Record a payment from a participant (EN)<br />
    ///     Ghi nhận thanh toán từ người tham gia (VI)
    /// </summary>
    /// <param name="participantId">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <param name="amount">Payment amount (EN)<br />Số tiền thanh toán (VI)</param>
    /// <param name="paymentMethod">Payment method (EN)<br />Phương thức thanh toán (VI)</param>
    /// <returns>Updated participant response DTO (EN)<br />DTO phản hồi người tham gia đã cập nhật (VI)</returns>
    Task<SharedExpenseParticipantResponseDto> RecordPaymentAsync(Guid participantId, decimal amount,
        string? paymentMethod = null);

    /// <summary>
    ///     Get unsettled participants for a shared expense (EN)<br />
    ///     Lấy người tham gia chưa thanh toán cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="sharedExpenseId">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>
    ///     List of unsettled participant response DTOs (EN)<br />Danh sách DTO phản hồi người tham gia chưa thanh toán
    ///     (VI)
    /// </returns>
    Task<IEnumerable<SharedExpenseParticipantResponseDto>> GetUnsettledParticipantsAsync(Guid sharedExpenseId);

    #endregion

    #region Calculation and Analysis

    /// <summary>
    ///     Calculate equal split amounts for a shared expense (EN)<br />
    ///     Tính số tiền chia đều cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="sharedExpenseId">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Equal split calculation result (EN)<br />Kết quả tính chia đều (VI)</returns>
    Task<EqualSplitCalculationDto> CalculateEqualSplitAsync(Guid sharedExpenseId);

    /// <summary>
    ///     Get expense summary for a shared expense (EN)<br />
    ///     Lấy tóm tắt chi tiêu cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="sharedExpenseId">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Expense summary DTO (EN)<br />DTO tóm tắt chi tiêu (VI)</returns>
    Task<SharedExpenseSummaryDto> GetExpenseSummaryAsync(Guid sharedExpenseId);

    /// <summary>
    ///     Get user's shared expense statistics (EN)<br />
    ///     Lấy thống kê chi tiêu chung của người dùng (VI)
    /// </summary>
    /// <returns>User statistics DTO (EN)<br />DTO thống kê người dùng (VI)</returns>
    Task<UserSharedExpenseStatsDto> GetUserStatisticsAsync();

    #endregion
}