using CoreFinance.Api.Constants;
using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using CoreFinance.Domain.Constants;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing recurring transaction templates. (EN)<br />
///     Controller để quản lý các mẫu giao dịch định kỳ. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = CoreFinanceAuthorizationPolicies.Owner)]
public class RecurringTransactionTemplateController(
    IRecurringTransactionTemplateService service,
    ILogger<RecurringTransactionTemplateController> logger)
    : CrudController<RecurringTransactionTemplate, RecurringTransactionTemplateCreateRequest,
        RecurringTransactionTemplateUpdateRequest, RecurringTransactionTemplateViewModel, Guid>(logger, service)
{
    /// <summary>
    ///     Gets paginated recurring transaction templates. (EN)<br />
    ///     Lấy danh sách mẫu giao dịch định kỳ có phân trang. (VI)
    /// </summary>
    /// <param name="request">
    ///     The request object containing pagination, filtering, and sorting details. (EN)<br />
    ///     Đối tượng request chứa thông tin phân trang, lọc và sắp xếp. (VI)
    /// </param>
    /// <returns>
    ///     A paginated list of recurring transaction templates if successful, or a status code indicating an error. (EN)<br />
    ///     Danh sách mẫu giao dịch định kỳ có phân trang nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<RecurringTransactionTemplateViewModel>>> GetPagingAsync(
        [FromBody] FilterBodyRequest request)
    {
        var result = await service.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     Gets the active templates of the authenticated owner. (EN)<br />
    ///     Lấy các mẫu giao dịch định kỳ đang hoạt động của owner đã xác thực. (VI)
    /// </summary>
    /// <returns>
    ///     A list of active recurring transaction templates. (EN)<br />
    ///     Danh sách các mẫu giao dịch định kỳ đang hoạt động. (VI)
    /// </returns>
    /// <remarks>
    ///     (EN) The owner comes only from the access token; the route accepts no user ID.<br />
    ///     (VI) Owner chỉ lấy từ access token; route không nhận ID người dùng.
    /// </remarks>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveTemplates()
    {
        var result = await service.GetActiveTemplatesAsync();
        return Ok(result);
    }

    /// <summary>
    ///     Gets templates by account ID. (EN)<br />
    ///     Lấy các mẫu giao dịch định kỳ theo ID tài khoản. (VI)
    /// </summary>
    /// <param name="accountId">
    ///     The ID of the account. (EN)<br />
    ///     ID của tài khoản. (VI)
    /// </param>
    /// <returns>
    ///     A list of recurring transaction templates for the specified account. (EN)<br />
    ///     Danh sách các mẫu giao dịch định kỳ cho tài khoản được chỉ định. (VI)
    /// </returns>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetTemplatesByAccount(Guid accountId)
    {
        var result = await service.GetTemplatesByAccountAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    ///     Toggles the active status of a recurring transaction template by its ID. (EN)<br />
    ///     Bật/tắt trạng thái hoạt động của mẫu giao dịch định kỳ dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template to toggle the active status for. (EN)<br />
    ///     ID của mẫu cần bật/tắt trạng thái hoạt động. (VI)
    /// </param>
    /// <param name="isActive">
    ///     The new active status. (EN)<br />
    ///     Trạng thái hoạt động mới. (VI)
    /// </param>
    /// <returns>
    ///     200 with {success,message} on success, or 404 with a message when the service returns false. (EN)<br />
    ///     200 với {success,message} khi thành công, hoặc 404 kèm thông báo khi dịch vụ trả false. (VI)
    /// </returns>
    [HttpPatch("{templateId:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveStatus(Guid templateId, [FromBody] bool isActive)
    {
        var result = await service.ToggleActiveStatusAsync(templateId, isActive, HttpContext.RequestAborted);
        if (!result)
            return NotFound(CoreFinanceMessages.TemplateNotFound);

        return Ok(new { success = true, message = CoreFinanceMessages.TemplateStatusUpdated });
    }

    /// <summary>
    ///     Soft deletes a template under its generation lock; generated periods are kept. (EN)<br />
    ///     Xóa mềm mẫu dưới khóa sinh; các kỳ đã sinh được giữ lại. (VI)
    /// </summary>
    /// <param name="id">(EN) The template to delete.<br />(VI) Mẫu cần xóa.</param>
    /// <returns>(EN) Empty 200 when deleted, 204 when not found.<br />(VI) 200 rỗng khi xóa thành công, 204 khi không tìm thấy.</returns>
    [HttpDelete("{id}")]
    public override async Task<ActionResult> Delete(Guid id)
    {
        return await service.DeleteTemplateAsync(id, HttpContext.RequestAborted) ? Ok() : NoContent();
    }

    /// <summary>
    ///     Calculates the next execution date for a recurring transaction template by its ID. (EN)<br />
    ///     Tính ngày thực hiện tiếp theo cho mẫu giao dịch định kỳ dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template. (EN)<br />
    ///     ID của mẫu. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result with the next execution date if successful, or NotFound if the template is not found. (EN)<br />
    ///     Kết quả Ok kèm theo ngày thực hiện tiếp theo nếu thành công, hoặc NotFound nếu không tìm thấy mẫu. (VI)
    /// </returns>
    /// <remarks>
    ///     (EN) Uses the StartDate-anchored slot after the cursor; ArgumentException maps to 404.<br />
    ///     (VI) Dùng slot sau cursor neo theo StartDate; ArgumentException được ánh xạ thành 404.
    /// </remarks>
    [HttpGet("{templateId:guid}/next-execution-date")]
    public async Task<IActionResult> CalculateNextExecutionDate(Guid templateId)
    {
        try
        {
            var nextDate = await service.CalculateNextExecutionDateAsync(templateId);
            return Ok(new { nextExecutionDate = nextDate });
        }
        catch (ArgumentException)
        {
            return NotFound(CoreFinanceMessages.TemplateNotFound);
        }
    }

    /// <summary>
    ///     Generates expected transactions for a specific recurring transaction template. (EN)<br />
    ///     Sinh các giao dịch dự kiến cho một mẫu giao dịch định kỳ cụ thể. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the template. (EN)<br />
    ///     ID của mẫu. (VI)
    /// </param>
    /// <param name="daysInAdvance">
    ///     Days after today UTC+7 to include; omission uses the template's stored DaysInAdvance. (EN)<br />
    ///     Số ngày sau hôm nay UTC+7 cần lấy; nếu bỏ qua thì dùng DaysInAdvance đã lưu của mẫu. (VI)
    /// </param>
    /// <returns>
    ///     200 with {success,message} after the generation transaction commits, or 400 with {error}. (EN)<br />
    ///     200 với {success,message} sau khi transaction sinh kỳ commit, hoặc 400 với {error}. (VI)
    /// </returns>
    [HttpPost("{templateId:guid}/generate-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactions(Guid templateId, [FromQuery] int? daysInAdvance = null)
    {
        try
        {
            await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance, HttpContext.RequestAborted);
            return Ok(new { success = true, message = CoreFinanceMessages.ExpectedTransactionsGenerated });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Generates expected transactions for the current owner's active auto-generate templates. (EN)<br />
    ///     Sinh giao dịch dự kiến cho các mẫu đang hoạt động và bật tự sinh của owner hiện tại. (VI)
    /// </summary>
    /// <returns>
    ///     200 with {success,message}, or 400 with {error}; successful templates remain committed if another fails. (EN)<br />
    ///     200 với {success,message}, hoặc 400 với {error}; mẫu thành công giữ commit nếu mẫu khác thất bại. (VI)
    /// </returns>
    [HttpPost("generate-all-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactionsForAllActiveTemplates()
    {
        try
        {
            await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync(HttpContext.RequestAborted);
            return Ok(new { success = true, message = CoreFinanceMessages.ExpectedTransactionsGeneratedForAllActiveTemplates });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}