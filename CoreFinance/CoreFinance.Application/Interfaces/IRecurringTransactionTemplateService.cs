using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Application.Interfaces;

/// <summary>
///     (EN) Represents the service interface for managing recurring transaction templates.<br />
///     (VI) Đại diện cho interface dịch vụ quản lý các mẫu giao dịch định kỳ.
/// </summary>
/// <remarks>
///     <para>
///         (EN) Queries and writes use the current owner's repository scope; client IDs never select another owner and
///         missing identity fails closed. Single-template create/update/toggle/delete/generation acquire the same
///         owner/template advisory lock inside their transaction before reading mutable state.<br />
///         (VI) Truy vấn và ghi dùng phạm vi repository của owner hiện tại; ID client không chọn owner khác và thiếu danh
///         tính sẽ bị từ chối. Tạo/cập nhật/bật tắt/xóa/sinh cho từng mẫu lấy cùng advisory lock theo owner/mẫu trong
///         transaction trước khi đọc trạng thái có thể thay đổi.
///     </para>
///     <para>
///         (EN) Inherited single-request CreateAsync validates the owned optional account, supplies a new ID and defaults
///         an omitted cursor to StartDate. It commits the template and first eligible generation window atomically, with
///         the repository supplying ownership. The inherited list overload retains the generic base contract.<br />
///         (VI) CreateAsync kế thừa cho một request kiểm tra tài khoản tùy chọn cùng owner, cấp ID mới và mặc định cursor
///         bị bỏ qua thành StartDate. Mẫu và cửa sổ sinh đầu tiên đủ điều kiện commit nguyên tử, repository gán owner.
///         Overload danh sách kế thừa giữ hợp đồng của lớp cơ sở dùng chung.
///     </para>
///     <para>
///         (EN) Inherited UpdateAsync applies supplied fields under the lock; omitted schedule/required fields retain
///         their stored values. Optional EndDate, Description, Category, Notes and CronExpression use full-form semantics:
///         supplied values are applied and omitted values are cleared because the Nuxt form omits empty values.
///         A changed StartDate resets the cursor to the new start; a changed frequency or applicable custom interval uses
///         min(old cursor, today UTC+7). Only missing
///         eligible periods are generated in the same transaction; existing periods, terminal states and tombstones retain
///         their dates, amounts and history.<br />
///         (VI) UpdateAsync kế thừa áp các trường được gửi dưới khóa; trường lịch/bắt buộc bị bỏ qua giữ giá trị đã lưu.
///         EndDate, Description, Category, Notes và CronExpression tùy chọn dùng ngữ nghĩa full-form: áp giá trị được gửi
///         và xóa giá trị bị bỏ qua vì form Nuxt không gửi giá trị rỗng.
///         Đổi StartDate đặt cursor về ngày bắt đầu mới; đổi tần suất hoặc khoảng tùy chỉnh có áp dụng dùng
///         min(cursor cũ, hôm nay UTC+7). Chỉ sinh kỳ còn thiếu đủ điều
///         kiện trong cùng transaction; kỳ đã có, trạng thái kết thúc và bản xóa mềm giữ ngày, số tiền và lịch sử.
///     </para>
///     <para>
///         (EN) Create/update return a view only after commit; validation, missing-reference, calendar and persistence
///         failures propagate after rollback. Update throws KeyNotFoundException for mismatched route/request IDs and
///         EntityNotFoundException for a missing/foreign template. Generation creates expected periods only, never actual
///         payments, and never recalculates balances.<br />
///         (VI) Tạo/cập nhật chỉ trả view sau commit; lỗi kiểm tra, thiếu tham chiếu, lịch và lưu trữ được ném lại sau
///         rollback. Cập nhật ném KeyNotFoundException khi ID route/request không khớp, EntityNotFoundException khi mẫu
///         thiếu/khác owner. Việc sinh chỉ tạo kỳ dự kiến, không tạo thanh toán thực và không tính lại số dư.
///     </para>
/// </remarks>
public interface IRecurringTransactionTemplateService : IBaseService<RecurringTransactionTemplate,
    RecurringTransactionTemplateCreateRequest, RecurringTransactionTemplateUpdateRequest,
    RecurringTransactionTemplateViewModel, Guid>
{
    /// <summary>
    ///     (EN) Gets a paginated list of recurring transaction templates.<br />
    ///     (VI) Lấy danh sách mẫu giao dịch định kỳ có phân trang.
    /// </summary>
    /// <param name="request">
    ///     (EN) Projected-field filters, ordering, paging and case-insensitive name/description/category search.<br />
    ///     (VI) Lọc trường đã chiếu, sắp xếp, phân trang và tìm tên/mô tả/danh mục không phân biệt hoa thường.
    /// </param>
    /// <returns>(EN) Owned views counted after filtering and clamped to valid pages.<br />(VI) View cùng owner, đếm sau lọc và kẹp vào trang hợp lệ.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IBasePaging<RecurringTransactionTemplateViewModel>?> GetPagingAsync(IFilterBodyRequest request);

    /// <summary>
    ///     (EN) Gets the active recurring transaction templates of the current owner.<br />
    ///     (VI) Lấy các mẫu giao dịch định kỳ đang hoạt động của owner hiện tại.
    /// </summary>
    /// <returns>(EN) The current owner's active templates; the owner comes only from the repository scope.<br />(VI) Các mẫu đang hoạt động của owner hiện tại; owner chỉ lấy từ phạm vi repository.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetActiveTemplatesAsync();

    /// <summary>
    ///     (EN) Gets a list of recurring transaction templates associated with a specific account.<br />
    ///     (VI) Lấy danh sách các mẫu giao dịch định kỳ liên quan đến một tài khoản cụ thể.
    /// </summary>
    /// <param name="accountId">(EN) The account ID to filter by.<br />(VI) ID tài khoản dùng để lọc.</param>
    /// <returns>(EN) Owned templates for the account, with owner-safe account metadata.<br />(VI) Mẫu cùng owner của tài khoản, với metadata tài khoản an toàn theo owner.</returns>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IEnumerable<RecurringTransactionTemplateViewModel>> GetTemplatesByAccountAsync(Guid accountId);

    /// <summary>
    ///     (EN) IDs of the current owner's active auto-generate templates, used to drive per-template generation.<br />
    ///     (VI) ID các mẫu đang hoạt động và tự sinh của owner hiện tại, dùng để điều phối việc sinh theo từng mẫu.
    /// </summary>
    /// <param name="cancellationToken">(EN) Cancels the template query.<br />(VI) Hủy truy vấn mẫu.</param>
    /// <returns>(EN) Matching owned, non-deleted template IDs ordered by identifier.<br />(VI) ID các mẫu cùng owner, chưa xóa, phù hợp, sắp theo ID.</returns>
    /// <exception cref="OperationCanceledException">(EN) The query is cancelled.<br />(VI) Truy vấn bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<IReadOnlyList<Guid>> GetActiveAutoGenerateTemplateIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Toggles the active status of a template under its generation lock.<br />
    ///     (VI) Bật/tắt trạng thái hoạt động của mẫu dưới khóa sinh.
    /// </summary>
    /// <param name="templateId">(EN) The template to toggle.<br />(VI) Mẫu cần bật/tắt.</param>
    /// <param name="isActive">(EN) The new active status.<br />(VI) Trạng thái hoạt động mới.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) True after commit; false for a missing/foreign template or a non-cancellation failure after transaction start.<br />
    ///     (VI) True sau commit; false nếu mẫu thiếu/khác owner hoặc có lỗi không phải hủy sau khi bắt đầu transaction.
    /// </returns>
    /// <exception cref="OperationCanceledException">(EN) The operation is cancelled; rollback still runs.<br />(VI) Thao tác bị hủy; rollback vẫn chạy.</exception>
    /// <remarks>
    ///     (EN) Generates no periods. Transaction-start and rollback failures propagate instead of returning false.<br />
    ///     (VI) Không sinh kỳ. Lỗi bắt đầu transaction và rollback được ném lại thay vì trả false.
    /// </remarks>
    Task<bool> ToggleActiveStatusAsync(Guid templateId, bool isActive, CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Soft deletes a template under its generation lock. Already generated periods are kept.<br />
    ///     (VI) Xóa mềm mẫu dưới khóa sinh. Các kỳ đã sinh được giữ nguyên.
    /// </summary>
    /// <param name="templateId">(EN) The template to delete.<br />(VI) Mẫu cần xóa.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) True after commit, false when no owned template is found; other failures propagate after rollback.<br />
    ///     (VI) True sau commit, false nếu không tìm thấy mẫu cùng owner; lỗi khác được ném lại sau rollback.
    /// </returns>
    /// <exception cref="OperationCanceledException">(EN) The operation is cancelled.<br />(VI) Thao tác bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<bool> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Calculates the next execution date for a recurring transaction template.<br />
    ///     (VI) Tính toán ngày thực hiện tiếp theo cho một mẫu giao dịch định kỳ.
    /// </summary>
    /// <remarks>
    ///     (EN) Returns the slot strictly after the stored cursor, not the cursor itself. Month-based slots anchor to
    ///     StartDate and clamp independently to each month's last day; day-based schedules add their interval.
    ///     Neither the cursor nor generated periods change.<br />
    ///     (VI) Trả slot ngay sau cursor đã lưu, không phải chính cursor. Slot theo tháng neo vào StartDate và kẹp độc lập
    ///     theo ngày cuối từng tháng; lịch theo ngày cộng khoảng ngày. Không thay đổi cursor hoặc các kỳ đã sinh.
    /// </remarks>
    /// <param name="templateId">(EN) The template whose schedule is read.<br />(VI) Mẫu cần đọc lịch.</param>
    /// <returns>(EN) The next nominal schedule date.<br />(VI) Ngày lịch danh nghĩa kế tiếp.</returns>
    /// <exception cref="ArgumentException">(EN) No owned template is found.<br />(VI) Không tìm thấy mẫu cùng owner.</exception>
    /// <exception cref="ArgumentOutOfRangeException">(EN) The stored custom day interval is not positive.<br />(VI) Khoảng ngày tùy chỉnh đã lưu không dương.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    Task<DateTime> CalculateNextExecutionDateAsync(Guid templateId);

    /// <summary>
    ///     (EN) Generates the missing expected transactions of one template in its own transaction, under its lock.<br />
    ///     (VI) Sinh các giao dịch dự kiến còn thiếu của một mẫu trong transaction riêng, dưới khóa của mẫu.
    /// </summary>
    /// <param name="templateId">(EN) The template to generate for.<br />(VI) Mẫu cần sinh kỳ.</param>
    /// <param name="daysInAdvance">(EN) Days after today UTC+7; null uses the template's stored window.<br />(VI) Số ngày sau hôm nay UTC+7; null dùng cửa sổ đã lưu của mẫu.</param>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>
    ///     (EN) Completes after commit; missing, foreign, inactive or manual-only templates generate nothing.<br />
    ///     (VI) Hoàn tất sau commit; mẫu thiếu, khác owner, không hoạt động hoặc chỉ sinh thủ công không tạo kỳ.
    /// </returns>
    /// <remarks>
    ///     (EN) Catches up from the cursor through today UTC+7 plus the window and EndDate, both inclusive, without a backlog
    ///     cap. Existing periods of every state, including tombstones, reserve slots; the cursor advances to the next
    ///     unprocessed slot. Failures propagate after rollback; no actual is created.<br />
    ///     (VI) Sinh bù từ cursor đến hôm nay UTC+7 cộng cửa sổ và EndDate, gồm cả hai biên, không giới hạn kỳ tồn đọng.
    ///     Kỳ đã có ở mọi trạng thái, kể cả bản xóa mềm, giữ chỗ; cursor tiến đến slot chưa xử lý kế tiếp.
    ///     Lỗi được ném lại sau rollback; không tạo giao dịch thực.
    /// </remarks>
    /// <exception cref="OperationCanceledException">(EN) The operation is cancelled.<br />(VI) Thao tác bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) No valid current owner exists.<br />(VI) Không có owner hiện tại hợp lệ.</exception>
    /// <exception cref="ArgumentOutOfRangeException">(EN) The stored custom interval or date arithmetic is out of range.<br />(VI) Khoảng tùy chỉnh đã lưu hoặc phép tính ngày vượt phạm vi hợp lệ.</exception>
    /// <exception cref="InvalidOperationException">(EN) The schedule does not advance.<br />(VI) Lịch không tiến về phía trước.</exception>
    /// <exception cref="Shared.Contracts.Exceptions.EntityNotFoundException">(EN) The template account is missing or foreign.<br />(VI) Tài khoản của mẫu không tồn tại hoặc thuộc owner khác.</exception>
    Task GenerateExpectedTransactionsAsync(Guid templateId, int? daysInAdvance = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     (EN) Generates for every active auto-generate template of the current owner, one isolated transaction per
    ///     template. Throws after the run when any template failed; the others stay committed.<br />
    ///     (VI) Sinh cho mọi mẫu đang hoạt động và tự sinh của owner hiện tại, mỗi mẫu một transaction riêng. Ném lỗi sau
    ///     khi chạy xong nếu có mẫu thất bại; các mẫu khác vẫn được commit.
    /// </summary>
    /// <param name="cancellationToken">(EN) Cancels the operation.<br />(VI) Hủy thao tác.</param>
    /// <returns>(EN) Completes after every eligible template has been attempted successfully.<br />(VI) Hoàn tất sau khi xử lý thành công mọi mẫu đủ điều kiện.</returns>
    /// <remarks>
    ///     (EN) Each template uses a fresh scope and its stored window. The caller must supply an owner identity available
    ///     to child scopes; HTTP uses the authenticated request. Cross-owner background work instead uses the worker's
    ///     separately initialized per-owner/template scopes.<br />
    ///     (VI) Mỗi mẫu dùng scope mới và cửa sổ đã lưu. Caller phải cung cấp danh tính owner khả dụng cho scope con;
    ///     đường HTTP dùng request đã xác thực. Công việc nền liên owner dùng các scope riêng theo owner/mẫu do worker khởi tạo.
    /// </remarks>
    /// <exception cref="InvalidOperationException">(EN) Some templates failed; successful ones remain committed.<br />(VI) Có mẫu thất bại; mẫu thành công vẫn giữ commit.</exception>
    /// <exception cref="OperationCanceledException">(EN) The operation is cancelled.<br />(VI) Thao tác bị hủy.</exception>
    /// <exception cref="UnauthorizedAccessException">(EN) The initial template catalogue has no valid current owner.<br />(VI) Danh mục mẫu ban đầu không có owner hiện tại hợp lệ.</exception>
    Task GenerateExpectedTransactionsForAllActiveTemplatesAsync(CancellationToken cancellationToken = default);
}