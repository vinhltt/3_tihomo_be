using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.RecurringTransactionTemplate;
using CoreFinance.Application.Interfaces;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing recurring transaction templates. (EN)<br />
///     Controller để quản lý các mẫu giao dịch định kỳ. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    ///     Gets active templates for a user by user ID. (EN)<br />
    ///     Lấy các mẫu giao dịch định kỳ đang hoạt động của người dùng theo ID người dùng. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <returns>
    ///     A list of active recurring transaction templates. (EN)<br />
    ///     Danh sách các mẫu giao dịch định kỳ đang hoạt động. (VI)
    /// </returns>
    [HttpGet("active/{userId:guid}")]
    public async Task<IActionResult> GetActiveTemplates(Guid userId)
    {
        var result = await service.GetActiveTemplatesAsync(userId);
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
    ///     An Ok result if successful, or NotFound if the template is not found. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc NotFound nếu không tìm thấy mẫu. (VI)
    /// </returns>
    [HttpPatch("{templateId:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveStatus(Guid templateId, [FromBody] bool isActive)
    {
        var result = await service.ToggleActiveStatusAsync(templateId, isActive);
        if (!result)
            return NotFound("Template not found");

        return Ok(new { success = true, message = "Template status updated successfully" });
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
            return NotFound("Template not found");
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
    ///     The number of days in advance to generate transactions for. Defaults to 30. (EN)<br />
    ///     Số ngày tạo giao dịch dự kiến trước. Mặc định là 30. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or BadRequest if an error occurs. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc BadRequest nếu xảy ra lỗi. (VI)
    /// </returns>
    [HttpPost("{templateId:guid}/generate-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactions(Guid templateId, [FromQuery] int daysInAdvance = 30)
    {
        try
        {
            await service.GenerateExpectedTransactionsAsync(templateId, daysInAdvance);
            return Ok(new { success = true, message = "Expected transactions generated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///     Generates expected transactions for all active recurring transaction templates. (EN)<br />
    ///     Sinh các giao dịch dự kiến cho tất cả các mẫu giao dịch định kỳ đang hoạt động. (VI)
    /// </summary>
    /// <returns>
    ///     An Ok result if successful, or BadRequest if an error occurs. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc BadRequest nếu xảy ra lỗi. (VI)
    /// </returns>
    [HttpPost("generate-all-expected-transactions")]
    public async Task<IActionResult> GenerateExpectedTransactionsForAllActiveTemplates()
    {
        try
        {
            await service.GenerateExpectedTransactionsForAllActiveTemplatesAsync();
            return Ok(new { success = true, message = "Expected transactions generated for all active templates" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}