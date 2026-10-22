using CoreFinance.Api.Constants;
using CoreFinance.Api.Controllers.Base;
using CoreFinance.Api.Infrastructures.Filters;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Enums;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Constants;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;
using Shared.EntityFramework.DTOs;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     (EN) Provides authenticated expected-transaction queries and lifecycle commands.<br />
///     (VI) Cung cấp truy vấn và lệnh vòng đời giao dịch dự kiến có xác thực.
/// </summary>
/// <remarks>
///     (EN) Error handling is local to this controller; other CRUD APIs retain their existing contracts.<br />
///     (VI) Xử lý lỗi chỉ áp dụng cục bộ cho controller này; các API CRUD khác giữ hợp đồng hiện có.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = CoreFinanceAuthorizationPolicies.Owner)]
[TypeFilter(typeof(ExpectedTransactionActionFilter), Order = int.MinValue)]
public class ExpectedTransactionController(
    IExpectedTransactionService service,
    ILogger<ExpectedTransactionController> logger)
    : CrudController<ExpectedTransaction, ExpectedTransactionCreateRequest,
        ExpectedTransactionUpdateRequest, ExpectedTransactionViewModel, Guid>(logger, service)
{
    /// <inheritdoc />
    /// <remarks>
    ///     (EN) Creates only an owned Pending period; the client cannot choose its owner.<br />
    ///     (VI) Chỉ tạo kỳ Pending thuộc owner hiện tại; client không được chọn owner.
    /// </remarks>
    [HttpPost]
    public override async Task<ActionResult> Post([FromForm] ExpectedTransactionCreateRequest? request)
    {
        if (request == null) return Error(StatusCodes.Status400BadRequest, CoreFinanceErrorCodes.InvalidRequest, CoreFinanceMessages.ExpectedTransactionRequestRequired);
        var result = await service.CreateAsync(request);
        return result == null ? Error(StatusCodes.Status500InternalServerError, CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.ExpectedTransactionSaveFailed) : Ok(result);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     (EN) Only an owned, unlinked Pending period can be edited; lifecycle transitions use commands.<br />
    ///     (VI) Chỉ sửa kỳ Pending cùng owner và chưa liên kết; chuyển trạng thái vòng đời phải dùng lệnh.
    /// </remarks>
    [HttpPut("{id}")]
    public override async Task<ActionResult> Put(Guid id, [FromForm] ExpectedTransactionUpdateRequest request)
    {
        if (id != request.Id) return Error(StatusCodes.Status400BadRequest, CoreFinanceErrorCodes.InvalidRequest, CoreFinanceMessages.RouteAndRequestIdsMustMatch);
        var result = await service.UpdateAsync(id, request);
        return result == null ? Error(StatusCodes.Status500InternalServerError, CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.ExpectedTransactionSaveFailed) : Ok(result);
    }

    /// <summary>
    ///     (EN) Soft-deletes an owned, unlinked Pending period using its original concurrency token.<br />
    ///     (VI) Xóa mềm kỳ Pending cùng owner, chưa liên kết bằng token đồng thời gốc.
    /// </summary>
    /// <param name="id">(EN) The expected period ID.<br />(VI) ID kỳ dự kiến.</param>
    /// <returns>
    ///     (EN) Empty 200 on success; otherwise a local {code,message} error, including 404 or 409.<br />
    ///     (VI) 200 rỗng khi thành công; ngược lại trả lỗi cục bộ {code,message}, gồm 404 hoặc 409.
    /// </returns>
    [HttpDelete("{id}")]
    public override async Task<ActionResult> Delete(Guid id)
    {
        var result = await service.DeletePendingAsync(id, HttpContext.RequestAborted);
        return result.Outcome == ExpectedTransactionActionOutcome.Success ? Ok() : ActionError(result);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     (EN) Returns 404 for missing/foreign periods without exposing foreign navigation details.<br />
    ///     (VI) Trả 404 cho kỳ không tồn tại/khác owner mà không lộ chi tiết tham chiếu của owner khác.
    /// </remarks>
    [HttpGet("{id}")]
    public override async Task<ActionResult> GetById(Guid id)
    {
        var result = await service.GetByIdAsync(id);
        return result == null ? Error(StatusCodes.Status404NotFound, CoreFinanceErrorCodes.ExpectedTransactionNotFound, CoreFinanceMessages.ExpectedTransactionNotFound) : Ok(result);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     (EN) Derived UTC+7 display state is filtered and counted in SQL before stable, clamped paging.<br />
    ///     (VI) Trạng thái hiển thị dẫn xuất theo UTC+7 được lọc và đếm trong SQL trước khi phân trang ổn định, kẹp hợp lệ.
    /// </remarks>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<ExpectedTransactionViewModel>>> GetPagingAsync(
        [FromBody] FilterBodyRequest request)
    {
        return Ok(await service.GetPagingAsync(request));
    }

    /// <summary>
    ///     (EN) Lists all owned Pending periods due through today UTC+7 plus seven days, including every overdue period.<br />
    ///     (VI) Liệt kê mọi kỳ Pending cùng owner có hạn đến hết hôm nay UTC+7 cộng bảy ngày, gồm toàn bộ kỳ quá hạn.
    /// </summary>
    /// <returns>(EN) 200 with {data,totalRow}; non-Pending periods are excluded.<br />(VI) 200 với {data,totalRow}; loại các kỳ không còn Pending.</returns>
    [HttpGet("reminders")]
    public async Task<IActionResult> GetReminders()
    {
        var data = await service.GetRemindersAsync(HttpContext.RequestAborted);
        return Ok(new { data, totalRow = data.Count });
    }

    /// <summary>
    ///     (EN) Lists the current owner's Pending periods, additionally filtered by the supplied user ID.<br />
    ///     (VI) Liệt kê kỳ Pending của owner hiện tại, lọc thêm theo ID người dùng được gửi.
    /// </summary>
    /// <param name="userId">(EN) An additional filter, never an authorization identity.<br />(VI) Bộ lọc bổ sung, không phải danh tính phân quyền.</param>
    /// <returns>(EN) 200 with matching periods ordered by expected date then ID.<br />(VI) 200 kèm kỳ phù hợp, sắp theo ngày dự kiến rồi ID.</returns>
    [HttpGet("pending/{userId:guid}")]
    public async Task<IActionResult> GetPendingTransactionsAsync(Guid userId)
    {
        return Ok(await service.GetPendingTransactionsAsync(userId));
    }

    /// <summary>
    ///     (EN) Lists owned Pending periods from today UTC+7 through the inclusive upcoming window.<br />
    ///     (VI) Liệt kê kỳ Pending cùng owner từ hôm nay UTC+7 đến hết cửa sổ sắp tới, gồm cả hai biên.
    /// </summary>
    /// <param name="userId">(EN) An additional owner filter; foreign IDs return no periods.<br />(VI) Bộ lọc owner bổ sung; ID khác owner không trả kỳ.</param>
    /// <param name="days">(EN) Days after today to include; defaults to 30.<br />(VI) Số ngày sau hôm nay cần lấy; mặc định 30.</param>
    /// <returns>(EN) 200 with matching upcoming periods, excluding overdue.<br />(VI) 200 kèm kỳ sắp tới phù hợp, không gồm kỳ quá hạn.</returns>
    [HttpGet("upcoming/{userId:guid}")]
    public async Task<IActionResult> GetUpcomingTransactions(Guid userId, [FromQuery] int days = 30)
    {
        return Ok(await service.GetUpcomingTransactionsAsync(userId, days));
    }

    /// <summary>
    ///     (EN) Lists the current owner's periods for a recurring template.<br />
    ///     (VI) Liệt kê các kỳ của owner hiện tại theo mẫu định kỳ.
    /// </summary>
    /// <param name="templateId">(EN) The template ID to filter by.<br />(VI) ID mẫu dùng để lọc.</param>
    /// <returns>(EN) 200 with matching periods, including terminal states.<br />(VI) 200 kèm kỳ phù hợp, gồm cả trạng thái kết thúc.</returns>
    [HttpGet("template/{templateId:guid}")]
    public async Task<IActionResult> GetTransactionsByTemplate(Guid templateId)
    {
        return Ok(await service.GetTransactionsByTemplateAsync(templateId));
    }

    /// <summary>
    ///     (EN) Lists the current owner's periods for an account.<br />
    ///     (VI) Liệt kê các kỳ của owner hiện tại theo tài khoản.
    /// </summary>
    /// <param name="accountId">(EN) The account ID to filter by.<br />(VI) ID tài khoản dùng để lọc.</param>
    /// <returns>(EN) 200 with matching periods, including terminal states.<br />(VI) 200 kèm kỳ phù hợp, gồm cả trạng thái kết thúc.</returns>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetTransactionsByAccount(Guid accountId)
    {
        return Ok(await service.GetTransactionsByAccountAsync(accountId));
    }

    /// <summary>
    ///     (EN) Lists owned periods in an inclusive range of nominal expected dates.<br />
    ///     (VI) Liệt kê kỳ cùng owner trong khoảng ngày dự kiến danh nghĩa, gồm cả hai biên.
    /// </summary>
    /// <param name="userId">(EN) An additional filter, never an authorization identity.<br />(VI) Bộ lọc bổ sung, không phải danh tính phân quyền.</param>
    /// <param name="startDate">(EN) Inclusive first date; time is ignored.<br />(VI) Ngày đầu tiên, có tính biên; bỏ qua giờ.</param>
    /// <param name="endDate">(EN) Inclusive last date; time is ignored.<br />(VI) Ngày cuối cùng, có tính biên; bỏ qua giờ.</param>
    /// <returns>(EN) 200 with matching periods in every stored state.<br />(VI) 200 kèm kỳ phù hợp ở mọi trạng thái lưu trữ.</returns>
    [HttpGet("date-range/{userId:guid}")]
    public async Task<IActionResult> GetTransactionsByDateRange(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return Ok(await service.GetTransactionsByDateRangeAsync(userId, startDate, endDate));
    }

    /// <summary>
    ///     (EN) Atomically creates an actual income/expense and confirms an owned, unlinked Pending period.<br />
    ///     (VI) Tạo giao dịch thu/chi thực và xác nhận kỳ Pending cùng owner, chưa liên kết một cách nguyên tử.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The period to pay, including overdue Pending.<br />(VI) Kỳ cần thanh toán, kể cả Pending quá hạn.</param>
    /// <param name="request">
    ///     (EN) An owned fallback account is required if unbound; a supplied bound account must match. Amount/date come from the period.<br />
    ///     (VI) Cần tài khoản dự phòng cùng owner nếu chưa gắn tài khoản; tài khoản gửi cho kỳ đã gắn phải khớp. Số tiền/ngày lấy từ kỳ.
    /// </param>
    /// <returns>
    ///     (EN) 200 with {success,expectedTransactionId,actualTransactionId,actualTransaction} after commit.
    ///     Failures use {code,message}, not the confirm/cancel/adjust success envelope.<br />
    ///     (VI) 200 với {success,expectedTransactionId,actualTransactionId,actualTransaction} sau commit.
    ///     Lỗi dùng {code,message}, không dùng cấu trúc thành công của xác nhận/hủy/điều chỉnh.
    /// </returns>
    [HttpPost("{expectedTransactionId:guid}/record-payment")]
    public async Task<IActionResult> RecordPayment(Guid expectedTransactionId, [FromBody] RecordPaymentRequest request)
    {
        var result = await service.RecordPaymentAsync(expectedTransactionId, request, HttpContext.RequestAborted);
        if (result.Outcome != ExpectedTransactionActionOutcome.Success) return ActionError(result);
        return Ok(new
        {
            success = true,
            expectedTransactionId = result.ExpectedTransactionId,
            actualTransactionId = result.ActualTransactionId,
            actualTransaction = result.ActualTransaction
        });
    }

    /// <summary>
    ///     (EN) Confirms an owned Pending period by linking an owned actual not linked elsewhere.<br />
    ///     (VI) Xác nhận kỳ Pending cùng owner bằng giao dịch thực cùng owner chưa liên kết nơi khác.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The unlinked period to confirm.<br />(VI) Kỳ chưa liên kết cần xác nhận.</param>
    /// <param name="request">(EN) An existing actual's ID; no new actual is created.<br />(VI) ID giao dịch thực có sẵn; không tạo giao dịch thực mới.</param>
    /// <returns>(EN) 200 with {success,message} after commit, or a {code,message} error.<br />(VI) 200 với {success,message} sau commit, hoặc lỗi {code,message}.</returns>
    [HttpPost("{expectedTransactionId:guid}/confirm")]
    public async Task<IActionResult> ConfirmExpectedTransaction(Guid expectedTransactionId,
        [FromBody] ConfirmTransactionRequest request)
    {
        var result = await service.ConfirmExpectedTransactionAsync(expectedTransactionId, request.ActualTransactionId,
            HttpContext.RequestAborted);
        return CommandResponse(result);
    }

    /// <summary>
    ///     (EN) Cancels an owned, unlinked Pending period with a required reason.<br />
    ///     (VI) Hủy kỳ Pending cùng owner, chưa liên kết với lý do bắt buộc.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The period to cancel.<br />(VI) Kỳ cần hủy.</param>
    /// <param name="request">(EN) The cancellation reason, stored as the adjustment reason.<br />(VI) Lý do hủy, lưu vào trường lý do điều chỉnh.</param>
    /// <returns>(EN) 200 with {success,message} after commit, or a {code,message} error.<br />(VI) 200 với {success,message} sau commit, hoặc lỗi {code,message}.</returns>
    [HttpPost("{expectedTransactionId:guid}/cancel")]
    public async Task<IActionResult> CancelExpectedTransaction(Guid expectedTransactionId,
        [FromBody] CancelTransactionRequest request)
    {
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, request.Reason,
            HttpContext.RequestAborted);
        return CommandResponse(result);
    }

    /// <summary>
    ///     (EN) Adjusts an owned, unlinked Pending period while retaining its first original amount.<br />
    ///     (VI) Chỉnh kỳ Pending cùng owner, chưa liên kết và giữ số gốc trước lần chỉnh đầu tiên.
    /// </summary>
    /// <param name="expectedTransactionId">(EN) The period to adjust.<br />(VI) Kỳ cần chỉnh.</param>
    /// <param name="request">(EN) A positive new amount and required reason.<br />(VI) Số tiền mới dương và lý do bắt buộc.</param>
    /// <returns>(EN) 200 with {success,message} after commit, or a {code,message} error.<br />(VI) 200 với {success,message} sau commit, hoặc lỗi {code,message}.</returns>
    [HttpPost("{expectedTransactionId:guid}/adjust")]
    public async Task<IActionResult> AdjustExpectedTransaction(Guid expectedTransactionId,
        [FromBody] AdjustTransactionRequest request)
    {
        var result = await service.AdjustExpectedTransactionAsync(expectedTransactionId, request.NewAmount, request.Reason,
            HttpContext.RequestAborted);
        return CommandResponse(result);
    }

    /// <summary>
    ///     (EN) Forecasts the owner's net Pending income minus expense; Transfer contributes zero.<br />
    ///     (VI) Dự báo thu Pending trừ chi Pending của owner; Transfer đóng góp 0.
    /// </summary>
    /// <param name="userId">(EN) An additional filter, never an authorization identity.<br />(VI) Bộ lọc bổ sung, không phải danh tính phân quyền.</param>
    /// <param name="startDate">(EN) Inclusive first expected date.<br />(VI) Ngày dự kiến đầu tiên, có tính biên.</param>
    /// <param name="endDate">(EN) Inclusive last expected date.<br />(VI) Ngày dự kiến cuối cùng, có tính biên.</param>
    /// <returns>(EN) 200 with {cashFlowForecast,startDate,endDate}.<br />(VI) 200 với {cashFlowForecast,startDate,endDate}.</returns>
    [HttpGet("cash-flow-forecast/{userId:guid}")]
    public async Task<IActionResult> GetCashFlowForecast(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);
        return Ok(new { cashFlowForecast = result, startDate, endDate });
    }

    /// <summary>
    ///     (EN) Forecasts categorized Pending totals: income is positive, every other type negative.<br />
    ///     (VI) Dự báo tổng Pending theo danh mục: thu mang dấu dương, mọi loại khác mang dấu âm.
    /// </summary>
    /// <param name="userId">(EN) An additional filter, never an authorization identity.<br />(VI) Bộ lọc bổ sung, không phải danh tính phân quyền.</param>
    /// <param name="startDate">(EN) Inclusive first expected date.<br />(VI) Ngày dự kiến đầu tiên, có tính biên.</param>
    /// <param name="endDate">(EN) Inclusive last expected date.<br />(VI) Ngày dự kiến cuối cùng, có tính biên.</param>
    /// <returns>
    ///     (EN) 200 with {categoryForecast,startDate,endDate}; uncategorized periods are excluded.<br />
    ///     (VI) 200 với {categoryForecast,startDate,endDate}; loại các kỳ không có danh mục.
    /// </returns>
    [HttpGet("category-forecast/{userId:guid}")]
    public async Task<IActionResult> GetCategoryForecast(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);
        return Ok(new { categoryForecast = result, startDate, endDate });
    }

    private ActionResult CommandResponse(ExpectedTransactionActionResult result)
    {
        return result.Outcome == ExpectedTransactionActionOutcome.Success
            ? Ok(new { success = true, message = result.Message })
            : ActionError(result);
    }

    private ActionResult ActionError(ExpectedTransactionActionResult result)
    {
        var status = result.Outcome switch
        {
            ExpectedTransactionActionOutcome.NotFound => StatusCodes.Status404NotFound,
            ExpectedTransactionActionOutcome.InvalidRequest => StatusCodes.Status400BadRequest,
            ExpectedTransactionActionOutcome.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
        return Error(status, result.Code, result.Message);
    }

    private ObjectResult Error(int status, string code, string message) => StatusCode(status, new { code, message });
}
