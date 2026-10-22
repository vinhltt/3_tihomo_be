namespace CoreFinance.Domain.Constants;

/// <summary>
///     (EN) Custom response, validation and exception text shared by CoreFinance layers.<br />
///     (VI) Nội dung phản hồi, kiểm tra và exception tùy chỉnh dùng chung giữa các tầng CoreFinance.
/// </summary>
/// <remarks>
///     (EN) Format templates retain callers' current-culture formatting; structured log templates stay at call sites.<br />
///     (VI) Mẫu định dạng giữ cách định dạng theo culture hiện tại của caller; mẫu log có cấu trúc vẫn ở nơi gọi.
/// </remarks>
public static class CoreFinanceMessages
{
    /// <summary>
    ///     (EN) The expected-period create endpoint received no request object.<br />
    ///     (VI) Endpoint tạo kỳ dự kiến không nhận được đối tượng request.
    /// </summary>
    public const string ExpectedTransactionRequestRequired = "An expected transaction request is required.";
    /// <summary>
    ///     (EN) Expected-period create/update returned no saved view; the API reports a persistence failure.<br />
    ///     (VI) Tạo/cập nhật kỳ dự kiến không trả view đã lưu; API báo lỗi lưu trữ.
    /// </summary>
    public const string ExpectedTransactionSaveFailed = "The expected transaction could not be saved.";
    /// <summary>
    ///     (EN) An update's route ID differs from its request ID.<br />
    ///     (VI) ID route của cập nhật khác ID trong request.
    /// </summary>
    public const string RouteAndRequestIdsMustMatch = "Route and request IDs must match.";
    /// <summary>
    ///     (EN) The period is missing or outside owner scope; the response does not distinguish the two.<br />
    ///     (VI) Kỳ không tồn tại hoặc ngoài phạm vi owner; phản hồi không phân biệt hai trường hợp.
    /// </summary>
    public const string ExpectedTransactionNotFound = "Expected transaction not found.";
    /// <summary>
    ///     (EN) Invalid model state is short-circuited before the expected action executes.<br />
    ///     (VI) Model-state không hợp lệ khiến request bị dừng trước khi action giao dịch dự kiến chạy.
    /// </summary>
    public const string MissingOrInvalidFields = "The request contains missing or invalid fields.";
    /// <summary>
    ///     (EN) Local not-found exception mapping hides whether the unavailable resource is a period or reference.<br />
    ///     (VI) Ánh xạ exception không tìm thấy cục bộ không tiết lộ tài nguyên thiếu là kỳ hay tham chiếu.
    /// </summary>
    public const string ExpectedTransactionOrResourceNotFound = "Expected transaction or referenced resource not found.";
    /// <summary>
    ///     (EN) A stale original concurrency token rejected a write; the caller should reload the period.<br />
    ///     (VI) Token đồng thời gốc đã cũ khiến thao tác ghi bị từ chối; caller cần tải lại kỳ.
    /// </summary>
    public const string PeriodChanged = "The period changed while being processed. Reload it before trying again.";
    /// <summary>
    ///     (EN) Another period or tombstone already reserves the requested actual-transaction link.<br />
    ///     (VI) Kỳ khác hoặc bản xóa mềm đã giữ chỗ liên kết giao dịch thực được yêu cầu.
    /// </summary>
    public const string ActualTransactionAlreadyLinked = "The actual transaction is already linked to a period.";
    /// <summary>
    ///     (EN) A unique-constraint conflict found an existing period for the template/calendar day.<br />
    ///     (VI) Xung đột ràng buộc duy nhất phát hiện kỳ đã có cho cặp mẫu/ngày lịch.
    /// </summary>
    public const string DuplicatePeriod = "A period already exists for that template and date.";
    /// <summary>
    ///     (EN) Input-related key, argument, format or overflow exceptions map to a safe request error.<br />
    ///     (VI) Exception về khóa, tham số, định dạng hoặc tràn giá trị đầu vào được ánh xạ sang lỗi request an toàn.
    /// </summary>
    public const string InvalidValues = "The request contains invalid values.";
    /// <summary>
    ///     (EN) An owner-access exception reached the expected API's local 401/403 boundary.<br />
    ///     (VI) Exception truy cập owner đi đến ranh giới 401/403 cục bộ của API giao dịch dự kiến.
    /// </summary>
    public const string AuthenticatedOwnerRequired = "An authenticated owner is required.";
    /// <summary>
    ///     (EN) Generic unexpected-action failure text that does not expose the exception or database details.<br />
    ///     (VI) Thông báo lỗi action ngoài dự kiến, không lộ exception hoặc chi tiết cơ sở dữ liệu.
    /// </summary>
    public const string OperationCouldNotBeCompleted = "The operation could not be completed.";
    /// <summary>
    ///     (EN) The owner-policy authentication challenge requires a bearer identity before action execution.<br />
    ///     (VI) Challenge xác thực của chính sách owner yêu cầu danh tính bearer trước khi chạy action.
    /// </summary>
    public const string AuthenticationRequired = "Authentication is required.";
    /// <summary>
    ///     (EN) Owner authorization rejects an authenticated request without a valid nonempty GUID owner claim.<br />
    ///     (VI) Phân quyền owner từ chối request đã xác thực nhưng thiếu claim owner là GUID hợp lệ khác rỗng.
    /// </summary>
    public const string ValidAuthenticatedOwnerClaimRequired = "A valid authenticated owner claim is required.";

    /// <summary>
    ///     (EN) A scope has neither an HTTP request nor an initialized background owner, so access fails closed.<br />
    ///     (VI) Scope không có HTTP request và cũng chưa khởi tạo owner nền nên truy cập bị từ chối.
    /// </summary>
    public const string NoUserContext = "No user context: no HTTP request and no background owner initialized for this scope.";
    /// <summary>
    ///     (EN) Background identity initialization is forbidden while an HTTP request owns the scope.<br />
    ///     (VI) Cấm khởi tạo danh tính nền khi HTTP request sở hữu scope.
    /// </summary>
    public const string BackgroundOwnerInsideHttpRequest = "A background owner cannot be set inside an HTTP request.";
    /// <summary>
    ///     (EN) A scope's background owner cannot be assigned a second time.<br />
    ///     (VI) Không được gán owner nền của scope lần thứ hai.
    /// </summary>
    public const string BackgroundOwnerAlreadyInitialized = "The background owner is already set for this scope.";
    /// <summary>
    ///     (EN) Background owner initialization rejects Guid.Empty.<br />
    ///     (VI) Khởi tạo owner nền từ chối Guid.Empty.
    /// </summary>
    public const string OwnerIdMustNotBeEmpty = "Owner ID must not be empty.";
    /// <summary>
    ///     (EN) HTTP owner resolution found no authenticated NameIdentifier with a nonempty GUID value.<br />
    ///     (VI) Phân giải owner HTTP không tìm thấy NameIdentifier đã xác thực có giá trị GUID khác rỗng.
    /// </summary>
    public const string AuthenticatedNameIdentifierRequired = "No authenticated NameIdentifier claim with a valid user ID.";
    /// <summary>
    ///     (EN) The owner/template advisory lock cannot be acquired without an open UOW transaction.<br />
    ///     (VI) Không thể lấy advisory lock owner/mẫu nếu chưa có transaction UOW đang mở.
    /// </summary>
    public const string TemplateLockRequiresTransaction = "The template lock requires an open transaction.";
    /// <summary>
    ///     (EN) A zero or negative custom day interval would prevent the recurrence schedule from advancing.<br />
    ///     (VI) Khoảng ngày tùy chỉnh bằng 0 hoặc âm khiến lịch định kỳ không tiến về phía trước.
    /// </summary>
    public const string CustomIntervalMustBePositive = "Custom interval must be greater than 0 days.";

    /// <summary>
    ///     (EN) Single-period creation wrote no entity; the create transaction must not report success.<br />
    ///     (VI) Tạo một kỳ không ghi được entity; transaction tạo không được báo thành công.
    /// </summary>
    public const string ExpectedTransactionCreateFailed = "Failed to create the expected transaction.";
    /// <summary>
    ///     (EN) The new owned period cannot be projected before commit, so its create transaction fails.<br />
    ///     (VI) Không đọc được projection kỳ mới cùng owner trước commit nên transaction tạo thất bại.
    /// </summary>
    public const string CreatedExpectedTransactionReadFailed = "The created expected transaction could not be read.";
    /// <summary>
    ///     (EN) Bulk expected-period creation wrote no entities and cannot commit as successful.<br />
    ///     (VI) Tạo hàng loạt kỳ dự kiến không ghi được entity và không thể commit như thành công.
    /// </summary>
    public const string ExpectedTransactionsCreateFailed = "Failed to create the expected transactions.";
    /// <summary>
    ///     (EN) A required account lookup found no account in the current owner's scope.<br />
    ///     (VI) Tra cứu tài khoản bắt buộc không tìm thấy tài khoản trong phạm vi owner hiện tại.
    /// </summary>
    public const string AccountNotFound = "Account not found.";
    /// <summary>
    ///     (EN) An expected-period request references no accessible owned recurring template.<br />
    ///     (VI) Request kỳ dự kiến tham chiếu mẫu định kỳ không tồn tại trong phạm vi owner có thể truy cập.
    /// </summary>
    public const string RecurringTransactionTemplateNotFound = "Recurring transaction template not found.";
    /// <summary>
    ///     (EN) Generic update rejects terminal/linked periods and direct lifecycle transitions.<br />
    ///     (VI) Cập nhật thông thường từ chối kỳ đã kết thúc/đã liên kết và chuyển trạng thái vòng đời trực tiếp.
    /// </summary>
    public const string PendingTransactionEditRequired = "Only an unlinked Pending transaction can be edited; use a command to change its status.";
    /// <summary>
    ///     (EN) A recurring period's date edit must retain its slot under the available owned template's current frequency.<br />
    ///     (VI) Sửa ngày kỳ định kỳ phải giữ slot theo tần suất hiện hành của mẫu cùng owner còn khả dụng.
    /// </summary>
    public const string ExpectedDateMustStayInSlot = "A recurring period can only be moved within its own schedule slot.";
    /// <summary>
    ///     (EN) Changing an expected amount through update requires a nonblank reason of at most 500 characters.<br />
    ///     (VI) Đổi số tiền dự kiến qua cập nhật cần lý do không chỉ chứa khoảng trắng, tối đa 500 ký tự.
    /// </summary>
    public const string AmountChangeReasonRequired = "An amount change requires a reason of at most 500 characters.";
    /// <summary>
    ///     (EN) Expected-period update wrote no entity; CRUD or command persistence fails.<br />
    ///     (VI) Cập nhật kỳ dự kiến không ghi được entity; việc lưu CRUD hoặc lệnh thất bại.
    /// </summary>
    public const string ExpectedTransactionUpdateFailed = "Failed to update the expected transaction.";
    /// <summary>
    ///     (EN) The updated period cannot be projected before commit, so the update rolls back.<br />
    ///     (VI) Không đọc được projection kỳ đã cập nhật trước commit nên cập nhật rollback.
    /// </summary>
    public const string UpdatedExpectedTransactionReadFailed = "The updated expected transaction could not be read.";
    /// <summary>
    ///     (EN) Payment received a null request or an account identifier rejected by structural validation.<br />
    ///     (VI) Thanh toán nhận request null hoặc ID tài khoản bị kiểm tra cấu trúc từ chối.
    /// </summary>
    public const string ValidPaymentRequestRequired = "A valid payment request is required.";
    /// <summary>
    ///     (EN) Record-payment rejects Transfer and other types that cannot create an actual income/expense.<br />
    ///     (VI) Ghi nhận thanh toán từ chối Transfer và loại khác không thể tạo giao dịch thu/chi thực.
    /// </summary>
    public const string PaymentTransactionTypeUnsupported = "Only income and expense periods can create a payment.";
    /// <summary>
    ///     (EN) Payment or adjustment rejects a zero or negative expected amount.<br />
    ///     (VI) Thanh toán hoặc điều chỉnh từ chối số tiền dự kiến bằng 0 hoặc âm.
    /// </summary>
    public const string ExpectedAmountMustBePositive = "The expected amount must be positive.";
    /// <summary>
    ///     (EN) A supplied payment account cannot replace the account already bound to the period.<br />
    ///     (VI) Tài khoản thanh toán được gửi không thể thay tài khoản đã gắn với kỳ.
    /// </summary>
    public const string BoundAccountCannotBeOverridden = "The bound account cannot be overridden.";
    /// <summary>
    ///     (EN) No nonempty account can be selected from the period and payment request.<br />
    ///     (VI) Không chọn được tài khoản có ID khác rỗng từ kỳ và request thanh toán.
    /// </summary>
    public const string PaymentAccountRequired = "Select an account to record this payment.";
    /// <summary>
    ///     (EN) Manual transaction creation returned no actual or an empty ID, so payment cannot commit.<br />
    ///     (VI) Đường tạo giao dịch tay không trả giao dịch thực hoặc trả ID rỗng nên thanh toán không thể commit.
    /// </summary>
    public const string ActualTransactionCreateFailed = "Failed to create the actual transaction.";
    /// <summary>
    ///     (EN) Success message returned only after the new actual and period confirmation commit together.<br />
    ///     (VI) Thông báo thành công chỉ trả sau khi giao dịch thực mới và xác nhận kỳ cùng commit.
    /// </summary>
    public const string PaymentRecorded = "Payment recorded successfully.";
    /// <summary>
    ///     (EN) Link-existing confirmation requires a nonempty actual-transaction ID.<br />
    ///     (VI) Xác nhận liên kết giao dịch có sẵn yêu cầu ID giao dịch thực khác rỗng.
    /// </summary>
    public const string ActualTransactionIdRequired = "An actual transaction ID is required.";
    /// <summary>
    ///     (EN) The requested actual is missing, soft-deleted or not owned by the period's owner.<br />
    ///     (VI) Giao dịch thực được yêu cầu không tồn tại, đã xóa mềm hoặc không thuộc owner của kỳ.
    /// </summary>
    public const string ActualTransactionNotFound = "Actual transaction not found.";
    /// <summary>
    ///     (EN) Success after committing confirmation with an existing actual link; no actual was created.<br />
    ///     (VI) Thành công sau commit xác nhận bằng liên kết giao dịch thực có sẵn; không tạo giao dịch thực.
    /// </summary>
    public const string ExpectedTransactionConfirmed = "Expected transaction confirmed successfully.";
    /// <summary>
    ///     (EN) Cancel/adjust requires a reason that is nonblank and no longer than 500 characters.<br />
    ///     (VI) Hủy/điều chỉnh yêu cầu lý do không chỉ chứa khoảng trắng và không quá 500 ký tự.
    /// </summary>
    public const string ReasonRequired = "A reason of at most 500 characters is required.";
    /// <summary>
    ///     (EN) Success after the cancellation state, reason and processing time commit.<br />
    ///     (VI) Thành công sau khi trạng thái hủy, lý do và thời điểm xử lý commit.
    /// </summary>
    public const string ExpectedTransactionCancelled = "Expected transaction cancelled successfully.";
    /// <summary>
    ///     (EN) Success after the adjustment commits while retaining the first original amount.<br />
    ///     (VI) Thành công sau khi điều chỉnh commit, vẫn giữ số gốc trước lần chỉnh đầu tiên.
    /// </summary>
    public const string ExpectedTransactionAdjusted = "Expected transaction adjusted successfully.";
    /// <summary>
    ///     (EN) Successful Pending soft-delete result; the HTTP delete success body remains empty.<br />
    ///     (VI) Kết quả xóa mềm Pending thành công; body HTTP xóa thành công vẫn rỗng.
    /// </summary>
    public const string ExpectedTransactionDeleted = "Expected transaction deleted successfully.";
    /// <summary>
    ///     (EN) A lifecycle command cannot process a terminal or already-linked period.<br />
    ///     (VI) Lệnh vòng đời không thể xử lý kỳ đã kết thúc hoặc đã liên kết.
    /// </summary>
    public const string PendingTransactionRequired = "Only an unlinked Pending transaction can be processed.";
    /// <summary>
    ///     (EN) A lifecycle command failed persistence; the response hides the underlying failure details.<br />
    ///     (VI) Lệnh vòng đời lưu thất bại; phản hồi che chi tiết lỗi bên dưới.
    /// </summary>
    public const string OperationCouldNotBeSaved = "The operation could not be saved.";

    /// <summary>
    ///     (EN) Recurring-controller 404 text for a false toggle result or next-date ArgumentException.<br />
    ///     (VI) Nội dung 404 của controller định kỳ khi bật/tắt trả false hoặc tính ngày kế tiếp ném ArgumentException.
    /// </summary>
    public const string TemplateNotFound = "Template not found";
    /// <summary>
    ///     (EN) The template active-state change committed successfully.<br />
    ///     (VI) Thay đổi trạng thái hoạt động của mẫu đã commit thành công.
    /// </summary>
    public const string TemplateStatusUpdated = "Template status updated successfully";
    /// <summary>
    ///     (EN) Single-template generation returned successfully, even when no eligible period was needed.<br />
    ///     (VI) Sinh kỳ cho một mẫu trả về thành công, kể cả khi không cần kỳ đủ điều kiện nào.
    /// </summary>
    public const string ExpectedTransactionsGenerated = "Expected transactions generated successfully";
    /// <summary>
    ///     (EN) Generation for the current owner's active auto-generate catalogue completed without a reported failure.<br />
    ///     (VI) Sinh kỳ cho danh mục mẫu hoạt động, bật tự sinh của owner hiện tại hoàn tất mà không báo lỗi.
    /// </summary>
    public const string ExpectedTransactionsGeneratedForAllActiveTemplates = "Expected transactions generated for all active templates";
    /// <summary>
    ///     (EN) Partial-generation failure: {0} is the failed-template count, {1} the total attempted-template count;
    ///     successful templates stay committed. Callers format with the current culture.<br />
    ///     (VI) Lỗi sinh một phần: {0} là số mẫu thất bại, {1} là tổng số mẫu đã thử xử lý;
    ///     mẫu thành công vẫn giữ commit. Caller định dạng theo culture hiện tại.
    /// </summary>
    public const string GenerationFailuresFormat = "Expected transaction generation failed for {0} of {1} templates; the others were generated.";
    /// <summary>
    ///     (EN) Recurring-template creation wrote no entity and must fail its transaction.<br />
    ///     (VI) Tạo mẫu định kỳ không ghi được entity nên transaction phải thất bại.
    /// </summary>
    public const string EntityCreateFailed = "Failed to create the entity";
    /// <summary>
    ///     (EN) An entity is missing from owner scope; {0} is its identifier (the template ID for recurring update),
    ///     formatted using the caller's current culture.<br />
    ///     (VI) Entity không có trong phạm vi owner; {0} là ID entity (ID mẫu khi cập nhật định kỳ),
    ///     được định dạng theo culture hiện tại của caller.
    /// </summary>
    public const string EntityNotFoundFormat = "Entity with ID {0} not found.";
    /// <summary>
    ///     (EN) Recurring-template update wrote no entity and must fail its transaction.<br />
    ///     (VI) Cập nhật mẫu định kỳ không ghi được entity nên transaction phải thất bại.
    /// </summary>
    public const string EntityUpdateFailed = "Failed to update the entity";
    /// <summary>
    ///     (EN) Generation detected a nonadvancing schedule; {0} is the template ID, formatted with the current culture.<br />
    ///     (VI) Việc sinh phát hiện lịch không tiến; {0} là ID mẫu, định dạng theo culture hiện tại.
    /// </summary>
    public const string ScheduleDoesNotAdvanceFormat = "Schedule of template {0} does not advance.";
    /// <summary>
    ///     (EN) No owned account matches a template reference; {0} is the account ID, formatted with the current culture.<br />
    ///     (VI) Không có tài khoản cùng owner khớp tham chiếu mẫu; {0} là ID tài khoản, định dạng theo culture hiện tại.
    /// </summary>
    public const string AccountNotFoundFormat = "Account {0} not found.";

    /// <summary>
    ///     (EN) An optional template reference was supplied as Guid.Empty rather than omitted.<br />
    ///     (VI) Tham chiếu mẫu tùy chọn được gửi dưới dạng Guid.Empty thay vì bỏ qua.
    /// </summary>
    public const string TemplateIdMustNotBeEmpty = "Template ID must not be empty.";
    /// <summary>
    ///     (EN) An optional account reference or payment selection was Guid.Empty; null is structurally allowed.<br />
    ///     (VI) Tham chiếu tài khoản hoặc lựa chọn thanh toán tùy chọn là Guid.Empty; null vẫn hợp lệ về cấu trúc.
    /// </summary>
    public const string AccountIdMustNotBeEmpty = "Account ID must not be empty.";
    /// <summary>
    ///     (EN) Expected-period creation rejects every initial state except Pending.<br />
    ///     (VI) Tạo kỳ dự kiến từ chối mọi trạng thái ban đầu ngoài Pending.
    /// </summary>
    public const string NewExpectedTransactionsMustBePending = "New expected transactions must be Pending.";
    /// <summary>
    ///     (EN) A supplied account code exceeds the 50-character validation limit.<br />
    ///     (VI) Mã tài khoản được gửi vượt giới hạn kiểm tra 50 ký tự.
    /// </summary>
    public const string AccountCodeMaxLength = "Account code must not exceed 50 characters / Mã tài khoản không được vượt quá 50 ký tự";
    /// <summary>
    ///     (EN) A supplied account code contains characters outside lowercase letters, digits and underscores.<br />
    ///     (VI) Mã tài khoản được gửi chứa ký tự ngoài chữ thường, chữ số và dấu gạch dưới.
    /// </summary>
    public const string AccountCodeSnakeCase = "Account code must be snake_case (lowercase letters, numbers, underscores only) / Mã tài khoản phải là snake_case (chỉ chữ thường, số, gạch dưới)";
    /// <summary>
    ///     (EN) An account code was supplied as an empty string rather than omitted.<br />
    ///     (VI) Mã tài khoản được gửi là chuỗi rỗng thay vì bỏ qua.
    /// </summary>
    public const string AccountCodeNotEmpty = "Account code cannot be empty string / Mã tài khoản không được là chuỗi rỗng";
}
