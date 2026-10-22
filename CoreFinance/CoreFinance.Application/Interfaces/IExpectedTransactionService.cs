using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Interfaces;

/// <summary>
///     (EN) Manages owner-scoped expected transactions and their lifecycle.<br />
///     (VI) Quản lý giao dịch dự kiến và vòng đời của chúng trong phạm vi owner.
/// </summary>
/// <remarks>
///     <para>
///         (EN) Access uses the current owner; client IDs only narrow scope. Inherited reads also project owner-safe
///         references and UTC+7 display state in SQL; missing/foreign IDs return no data.<br />
///         (VI) Truy cập dùng owner hiện tại; ID client chỉ thu hẹp phạm vi. Truy vấn kế thừa cũng chiếu tham chiếu an toàn
///         theo owner và trạng thái hiển thị UTC+7 trong SQL; ID thiếu/khác owner không trả dữ liệu.
///     </para>
///     <para>
///         (EN) Inherited single/bulk CreateAsync accepts Pending with owned optional references; the repository supplies
///         owner, ignoring client UserId. Each call commits atomically without actuals; empty bulk returns an empty collection.<br />
///         (VI) CreateAsync đơn/hàng loạt kế thừa chỉ nhận Pending với tham chiếu tùy chọn cùng owner; repository gán owner,
///         bỏ qua UserId client. Từng lần gọi commit nguyên tử, không tạo giao dịch thực; đầu vào hàng loạt rỗng trả tập rỗng.
///     </para>
///     <para>
///         (EN) Inherited UpdateAsync requires unlinked Pending, preserving owner/references/original tracked xmin.
///         Positive amount changes require a reason and retain the first original amount; date/metadata edits retain state.
///         A date change linked to an available owned, non-deleted template must stay in the same schedule slot under its
///         current frequency; otherwise UpdateAsync throws FluentValidation.ValidationException before any entity mutation.
///         Unbound periods and periods with missing/foreign/soft-deleted templates have no slot restriction.
///         It reads the updated view before commit. CRUD validation/reference/state/persistence failures remain exceptions;
///         mismatched route/request IDs throw KeyNotFoundException.<br />
///         (VI) UpdateAsync kế thừa yêu cầu Pending chưa liên kết, giữ owner/tham chiếu/xmin gốc đang theo dõi.
///         Đổi số tiền dương cần lý do và giữ số gốc lần đầu; sửa ngày/metadata giữ trạng thái.
///         Đổi ngày kỳ gắn với mẫu cùng owner còn khả dụng, chưa xóa mềm phải giữ cùng slot theo tần suất hiện hành của mẫu;
///         nếu không, UpdateAsync ném FluentValidation.ValidationException trước mọi thay đổi entity.
///         Kỳ không gắn mẫu hoặc có mẫu thiếu/khác owner/đã xóa mềm không bị giới hạn slot.
///         View cập nhật được đọc trước commit. Lỗi kiểm tra/tham chiếu/trạng thái/lưu trữ của CRUD vẫn là exception;
///         ID route/request không khớp ném KeyNotFoundException.
///     </para>
/// </remarks>
public interface IExpectedTransactionService : IBaseService<ExpectedTransaction,
    ExpectedTransactionCreateRequest, ExpectedTransactionUpdateRequest,
    ExpectedTransactionViewModel, Guid>
{
    /// <summary>
    ///     (EN) Gets a paginated list of expected transactions.<br />
    ///     (VI) Lấy danh sách giao dịch dự kiến có phân trang.
    /// </summary>
    /// <param name="request">(EN) Search, projected-field filters, ordering and paging to normalize.<br />(VI) Tìm kiếm, lọc trường đã chiếu, thứ tự và phân trang cần chuẩn hóa.</param>
    /// <returns>
    ///     (EN) Owned views with UTC+7 display state filtered/countable in SQL, clamped pages and an ID ordering tie-breaker.<br />
    ///     (VI) View cùng owner với trạng thái UTC+7 được lọc/đếm trong SQL, trang được kẹp hợp lệ và ID làm tiêu chí sắp xếp phụ.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IBasePaging<ExpectedTransactionViewModel>?> GetPagingAsync(IFilterBodyRequest request);

    /// <summary>
    ///     (EN) Gets every owned Pending period due through today UTC+7 plus seven days, including all overdue periods.<br />
    ///     (VI) Lấy mọi kỳ Pending cùng owner có hạn đến hết hôm nay UTC+7 cộng bảy ngày, gồm toàn bộ kỳ quá hạn.
    /// </summary>
    /// <param name="cancellationToken">(EN) Cancels the database query.<br />(VI) Hủy truy vấn cơ sở dữ liệu.</param>
    /// <returns>
    ///     (EN) UTC+7-derived views ordered by expected date then ID, without a lower date bound or result cap.<br />
    ///     (VI) View dẫn xuất theo UTC+7, sắp theo ngày dự kiến rồi ID, không có cận ngày dưới hoặc giới hạn số kết quả.
    /// </returns>
    /// <exception cref="OperationCanceledException">(EN) The query is cancelled.<br />(VI) Truy vấn bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IReadOnlyList<ExpectedTransactionViewModel>> GetRemindersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Creates an actual income or expense and confirms the Pending period in one transaction.<br />
    ///     (VI) Tạo giao dịch thu hoặc chi thực và xác nhận kỳ Pending trong cùng transaction.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The owned, unlinked Pending period, including overdue.<br />(VI) Kỳ Pending cùng owner, chưa liên kết, kể cả quá hạn.</param>
    /// <param name="request">(EN) Owned fallback account selection for an unbound period.<br />(VI) Lựa chọn tài khoản dự phòng cùng owner cho kỳ chưa gắn tài khoản.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation; rollback uses an uncancelled token.<br />(VI) Hủy thao tác; rollback dùng token không bị hủy.</param>
    /// <returns>
    ///     (EN) Success only after commit, carrying the new actual and both IDs; otherwise a classified lifecycle failure.<br />
    ///     (VI) Chỉ trả Success sau commit, kèm giao dịch thực mới và cả hai ID; ngược lại trả lỗi vòng đời đã phân loại.
    /// </returns>
    /// <remarks>
    ///     (EN) Amount/date come from the period. The service requires an owned fallback if unbound; a supplied bound
    ///     account must match. The fallback is used for the actual, not rebound onto the period. Transfer is unsupported.
    ///     Manual transaction creation shares the outer UOW, original tracked xmin is retained, and failures roll back
    ///     actual creation and confirmation together; account balances are not recalculated.<br />
    ///     (VI) Số tiền/ngày lấy từ kỳ. Dịch vụ yêu cầu tài khoản dự phòng cùng owner nếu chưa gắn tài khoản; tài khoản được
    ///     gửi cho kỳ đã gắn phải khớp. Tài khoản dự phòng chỉ dùng cho giao dịch thực, không gắn lại vào kỳ. Không hỗ trợ Transfer.
    ///     Đường tạo giao dịch tay dùng chung UOW ngoài, giữ xmin gốc đang theo dõi; khi lỗi, việc tạo giao dịch thực và xác nhận
    ///     cùng rollback; không tính lại số dư tài khoản.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) Cancellation is requested before commit.<br />(VI) Có yêu cầu hủy trước commit.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<ExpectedTransactionActionResult> RecordPaymentAsync(Guid expectedTransactionId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Soft-deletes an owned Pending period using its original concurrency token.<br />
    ///     (VI) Xóa mềm kỳ Pending cùng owner bằng token đồng thời gốc.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The owned, unlinked Pending period to delete.<br />(VI) Kỳ Pending cùng owner, chưa liên kết cần xóa.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>(EN) Success after commit, or NotFound, Conflict or PersistenceError.<br />(VI) Success sau commit, hoặc NotFound, Conflict hay PersistenceError.</returns>
    /// <remarks>
    ///     (EN) The tombstone reserves its generation slot; no actual is created. Failures roll back without a cancelled token.<br />
    ///     (VI) Bản xóa mềm giữ chỗ kỳ sinh; không tạo giao dịch thực. Khi lỗi, rollback không dùng token đã bị hủy.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) Cancellation is requested before commit.<br />(VI) Có yêu cầu hủy trước commit.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<ExpectedTransactionActionResult> DeletePendingAsync(Guid expectedTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Gets a list of pending expected transactions for a user.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến đang chờ xử lý cho người dùng.
    /// </summary>
    /// <param name="userId">(EN) Narrows, never replaces, the current owner scope.<br />(VI) Chỉ thu hẹp, không thay thế phạm vi owner hiện tại.</param>
    /// <returns>(EN) Owned Pending views, including overdue, ordered by expected date then ID.<br />(VI) View Pending cùng owner, kể cả quá hạn, sắp theo ngày dự kiến rồi ID.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetPendingTransactionsAsync(Guid userId);

    /// <summary>
    ///     (EN) Gets a list of upcoming expected transactions for a user within a specified number of days.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến sắp tới cho người dùng trong một số ngày cụ thể.
    /// </summary>
    /// <param name="userId">(EN) Narrows, never replaces, the current owner scope.<br />(VI) Chỉ thu hẹp, không thay thế phạm vi owner hiện tại.</param>
    /// <param name="days">(EN) Days after today UTC+7 to include; defaults to 30.<br />(VI) Số ngày sau hôm nay UTC+7 cần lấy; mặc định 30.</param>
    /// <returns>
    ///     (EN) Owned Pending views from today through today plus days, inclusive, excluding overdue; ordered by date then ID.<br />
    ///     (VI) View Pending cùng owner từ hôm nay đến hôm nay cộng days, gồm hai biên, không gồm quá hạn; sắp theo ngày rồi ID.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetUpcomingTransactionsAsync(Guid userId, int days = 30);

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific recurring transaction template.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một mẫu giao dịch định kỳ cụ thể.
    /// </summary>
    /// <param name="templateId">(EN) The recurring template ID to filter by.<br />(VI) ID mẫu định kỳ dùng để lọc.</param>
    /// <returns>(EN) Owned views in any stored state for the template, ordered by expected date then ID.<br />(VI) View cùng owner ở mọi trạng thái lưu trữ của mẫu, sắp theo ngày dự kiến rồi ID.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByTemplateAsync(Guid templateId);

    /// <summary>
    ///     (EN) Gets a list of expected transactions associated with a specific account.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">(EN) The account ID to filter by.<br />(VI) ID tài khoản dùng để lọc.</param>
    /// <returns>(EN) Owned views in any stored state for the account, ordered by expected date then ID.<br />(VI) View cùng owner ở mọi trạng thái lưu trữ của tài khoản, sắp theo ngày dự kiến rồi ID.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByAccountAsync(Guid accountId);

    /// <summary>
    ///     (EN) Gets a list of expected transactions for a user within a specified date range.<br />
    ///     (VI) Lấy danh sách các giao dịch dự kiến cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">(EN) Narrows, never replaces, the current owner scope.<br />(VI) Chỉ thu hẹp, không thay thế phạm vi owner hiện tại.</param>
    /// <param name="startDate">(EN) Inclusive first nominal date; time/kind are ignored.<br />(VI) Ngày danh nghĩa đầu tiên, có tính biên; bỏ qua giờ/kind.</param>
    /// <param name="endDate">(EN) Inclusive last nominal date; time/kind are ignored.<br />(VI) Ngày danh nghĩa cuối cùng, có tính biên; bỏ qua giờ/kind.</param>
    /// <returns>(EN) Owned views in every stored state, ordered by expected date then ID.<br />(VI) View cùng owner ở mọi trạng thái lưu trữ, sắp theo ngày dự kiến rồi ID.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<ExpectedTransactionViewModel>> GetTransactionsByDateRangeAsync(Guid userId, DateTime startDate,
        DateTime endDate);

    /// <summary>
    ///     (EN) Confirms an owned Pending period by linking an owned actual not used by another period.<br />
    ///     (VI) Xác nhận kỳ Pending cùng owner bằng cách liên kết giao dịch thực cùng owner chưa được kỳ khác sử dụng.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The owned, unlinked Pending period to confirm.<br />(VI) Kỳ Pending cùng owner, chưa liên kết cần xác nhận.</param>
    /// <param name="actualTransactionId">(EN) An owned actual not reserved by another period or tombstone.<br />(VI) Giao dịch thực cùng owner chưa được kỳ khác hoặc bản xóa mềm giữ chỗ.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) Success after commit with the linked ID but no new actual projection, or a classified lifecycle failure.<br />
    ///     (VI) Success sau commit kèm ID liên kết nhưng không có projection giao dịch thực mới, hoặc lỗi vòng đời đã phân loại.
    /// </returns>
    /// <remarks>
    ///     (EN) Creates no actual; Transfer may use this path. Confirmation records processing time and retains original
    ///     tracked xmin; failures roll back without a cancelled token.<br />
    ///     (VI) Không tạo giao dịch thực; Transfer có thể dùng đường này. Xác nhận ghi thời điểm xử lý và giữ xmin gốc
    ///     đang theo dõi; khi lỗi, rollback không dùng token đã bị hủy.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) Cancellation is requested before commit.<br />(VI) Có yêu cầu hủy trước commit.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<ExpectedTransactionActionResult> ConfirmExpectedTransactionAsync(Guid expectedTransactionId,
        Guid actualTransactionId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Cancels an owned Pending period with a required reason.<br />
    ///     (VI) Hủy kỳ Pending cùng owner với lý do bắt buộc.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The owned, unlinked Pending period to cancel.<br />(VI) Kỳ Pending cùng owner, chưa liên kết cần hủy.</param>
    /// <param name="reason">(EN) A nonblank reason of at most 500 characters.<br />(VI) Lý do không chỉ chứa khoảng trắng, tối đa 500 ký tự.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) Success after committing Cancelled, reason and processing time without an actual, or a classified failure.<br />
    ///     (VI) Success sau khi commit Cancelled, lý do và thời điểm xử lý mà không tạo giao dịch thực, hoặc lỗi đã phân loại.
    /// </returns>
    /// <remarks>
    ///     (EN) Retains original tracked xmin; failures roll back without a cancelled token.<br />
    ///     (VI) Giữ xmin gốc đang theo dõi; khi lỗi, rollback không dùng token đã bị hủy.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) Cancellation is requested before commit.<br />(VI) Có yêu cầu hủy trước commit.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<ExpectedTransactionActionResult> CancelExpectedTransactionAsync(Guid expectedTransactionId, string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Adjusts a Pending period to a positive amount, retaining the first original amount.<br />
    ///     (VI) Chỉnh kỳ Pending sang số tiền dương, giữ số gốc trước lần chỉnh đầu tiên.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The owned, unlinked Pending period to adjust.<br />(VI) Kỳ Pending cùng owner, chưa liên kết cần chỉnh.</param>
    /// <param name="newAmount">(EN) A strictly positive replacement amount.<br />(VI) Số tiền thay thế bắt buộc lớn hơn 0.</param>
    /// <param name="reason">(EN) A nonblank reason of at most 500 characters.<br />(VI) Lý do không chỉ chứa khoảng trắng, tối đa 500 ký tự.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) Success after committing the new amount/reason while retaining Pending and the first original amount, or a classified failure.<br />
    ///     (VI) Success sau khi commit số tiền/lý do mới, giữ Pending và số gốc lần đầu, hoặc lỗi đã phân loại.
    /// </returns>
    /// <remarks>
    ///     (EN) Creates no actual and retains original tracked xmin; failures roll back without a cancelled token.<br />
    ///     (VI) Không tạo giao dịch thực và giữ xmin gốc đang theo dõi; khi lỗi, rollback không dùng token đã bị hủy.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) Cancellation is requested before commit.<br />(VI) Có yêu cầu hủy trước commit.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<ExpectedTransactionActionResult> AdjustExpectedTransactionAsync(Guid expectedTransactionId, decimal newAmount,
        string reason, CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Gets the cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">(EN) Narrows, never replaces, the current owner scope.<br />(VI) Chỉ thu hẹp, không thay thế phạm vi owner hiện tại.</param>
    /// <param name="startDate">(EN) Inclusive first nominal expected date.<br />(VI) Ngày dự kiến danh nghĩa đầu tiên, có tính biên.</param>
    /// <param name="endDate">(EN) Inclusive last nominal expected date.<br />(VI) Ngày dự kiến danh nghĩa cuối cùng, có tính biên.</param>
    /// <returns>(EN) Pending income minus expense; Transfer contributes zero.<br />(VI) Thu Pending trừ chi Pending; Transfer đóng góp 0.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<decimal> GetCashFlowForecastAsync(Guid userId, DateTime startDate, DateTime endDate);

    /// <summary>
    ///     (EN) Gets the category-based cash flow forecast for a user within a specified date range.<br />
    ///     (VI) Lấy dự báo dòng tiền theo danh mục cho người dùng trong một khoảng ngày cụ thể.
    /// </summary>
    /// <param name="userId">(EN) Narrows, never replaces, the current owner scope.<br />(VI) Chỉ thu hẹp, không thay thế phạm vi owner hiện tại.</param>
    /// <param name="startDate">(EN) Inclusive first nominal expected date.<br />(VI) Ngày dự kiến danh nghĩa đầu tiên, có tính biên.</param>
    /// <param name="endDate">(EN) Inclusive last nominal expected date.<br />(VI) Ngày dự kiến danh nghĩa cuối cùng, có tính biên.</param>
    /// <returns>
    ///     (EN) Pending totals by nonempty category; income is positive, all other transaction types negative.<br />
    ///     (VI) Tổng Pending theo danh mục không rỗng; thu mang dấu dương, mọi loại giao dịch khác mang dấu âm.
    /// </returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<Dictionary<string, decimal>> GetCategoryForecastAsync(Guid userId, DateTime startDate, DateTime endDate);
}