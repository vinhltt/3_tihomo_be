using CoreFinance.Api.Controllers.Base;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Application.Interfaces;
using Shared.EntityFramework.DTOs;
using CoreFinance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;

namespace CoreFinance.Api.Controllers;

/// <summary>
///     Controller for managing expected transactions. (EN)<br />
///     Controller để quản lý các giao dịch dự kiến. (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExpectedTransactionController(
    IExpectedTransactionService service,
    ILogger<ExpectedTransactionController> logger)
    : CrudController<ExpectedTransaction, ExpectedTransactionCreateRequest,
        ExpectedTransactionUpdateRequest, ExpectedTransactionViewModel, Guid>(logger, service)
{
    /// <summary>
    ///     Gets paginated expected transactions based on filtering and sorting criteria. (EN)<br />
    ///     Lấy danh sách các giao dịch dự kiến có phân trang dựa trên tiêu chí lọc và sắp xếp. (VI)
    /// </summary>
    /// <param name="request">
    ///     The request object containing pagination, filtering, and sorting details. (EN)<br />
    ///     Đối tượng request chứa thông tin phân trang, lọc và sắp xếp. (VI)
    /// </param>
    /// <returns>
    ///     A paginated list of expected transactions if successful, or a status code indicating an error. (EN)<br />
    ///     Danh sách giao dịch dự kiến có phân trang nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPost("filter")]
    public override async Task<ActionResult<IBasePaging<ExpectedTransactionViewModel>>> GetPagingAsync(
        [FromBody] FilterBodyRequest request)
    {
        var result = await service.GetPagingAsync(request);
        return Ok(result);
    }

    /// <summary>
    ///     Gets pending expected transactions for a user by user ID. (EN)<br />
    ///     Lấy các giao dịch dự kiến đang chờ xử lý của người dùng theo ID người dùng. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <returns>
    ///     A list of pending expected transactions. (EN)<br />
    ///     Danh sách các giao dịch dự kiến đang chờ xử lý. (VI)
    /// </returns>
    [HttpGet("pending/{userId:guid}")]
    public async Task<IActionResult> GetPendingTransactionsAsync(Guid userId)
    {
        var result = await service.GetPendingTransactionsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    ///     Gets upcoming expected transactions for a user within a specified number of days. (EN)<br />
    ///     Lấy các giao dịch dự kiến sắp tới của người dùng trong một số ngày được chỉ định. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <param name="days">
    ///     The number of upcoming days to retrieve transactions for. Defaults to 30. (EN)<br />
    ///     Số ngày sắp tới để lấy giao dịch. Mặc định là 30. (VI)
    /// </param>
    /// <returns>
    ///     A list of upcoming expected transactions. (EN)<br />
    ///     Danh sách các giao dịch dự kiến sắp tới. (VI)
    /// </returns>
    [HttpGet("upcoming/{userId:guid}")]
    public async Task<IActionResult> GetUpcomingTransactions(Guid userId, [FromQuery] int days = 30)
    {
        var result = await service.GetUpcomingTransactionsAsync(userId, days);
        return Ok(result);
    }

    /// <summary>
    ///     Gets expected transactions by recurring transaction template ID. (EN)<br />
    ///     Lấy các giao dịch dự kiến theo ID mẫu giao dịch định kỳ. (VI)
    /// </summary>
    /// <param name="templateId">
    ///     The ID of the recurring transaction template. (EN)<br />
    ///     ID của mẫu giao dịch định kỳ. (VI)
    /// </param>
    /// <returns>
    ///     A list of expected transactions associated with the specified template. (EN)<br />
    ///     Danh sách các giao dịch dự kiến liên quan đến mẫu được chỉ định. (VI)
    /// </returns>
    [HttpGet("template/{templateId:guid}")]
    public async Task<IActionResult> GetTransactionsByTemplate(Guid templateId)
    {
        var result = await service.GetTransactionsByTemplateAsync(templateId);
        return Ok(result);
    }

    /// <summary>
    ///     Gets expected transactions by account ID. (EN)<br />
    ///     Lấy các giao dịch dự kiến theo ID tài khoản. (VI)
    /// </summary>
    /// <param name="accountId">
    ///     The ID of the account. (EN)<br />
    ///     ID của tài khoản. (VI)
    /// </param>
    /// <returns>
    ///     A list of expected transactions associated with the specified account. (EN)<br />
    ///     Danh sách các giao dịch dự kiến liên quan đến tài khoản được chỉ định. (VI)
    /// </returns>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetTransactionsByAccount(Guid accountId)
    {
        var result = await service.GetTransactionsByAccountAsync(accountId);
        return Ok(result);
    }

    /// <summary>
    ///     Gets expected transactions for a user within a specified date range. (EN)<br />
    ///     Lấy các giao dịch dự kiến của người dùng trong một khoảng thời gian được chỉ định. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <param name="startDate">
    ///     The start date of the range. (EN)<br />
    ///     Ngày bắt đầu của khoảng thời gian. (VI)
    /// </param>
    /// <param name="endDate">
    ///     The end date of the range. (EN)<br />
    ///     Ngày kết thúc của khoảng thời gian. (VI)
    /// </param>
    /// <returns>
    ///     A list of expected transactions within the specified date range. (EN)<br />
    ///     Danh sách các giao dịch dự kiến trong khoảng thời gian được chỉ định. (VI)
    /// </returns>
    [HttpGet("date-range/{userId:guid}")]
    public async Task<IActionResult> GetTransactionsByDateRange(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await service.GetTransactionsByDateRangeAsync(userId, startDate, endDate);
        return Ok(result);
    }

    /// <summary>
    ///     Confirms an expected transaction and links it to an actual transaction. (EN)<br />
    ///     Xác nhận một giao dịch dự kiến và liên kết nó với một giao dịch thực tế. (VI)
    /// </summary>
    /// <param name="expectedTransactionId">
    ///     The ID of the expected transaction to confirm. (EN)<br />
    ///     ID của giao dịch dự kiến cần xác nhận. (VI)
    /// </param>
    /// <param name="request">
    ///     The request containing the ID of the actual transaction to link. (EN)<br />
    ///     Request chứa ID của giao dịch thực tế để liên kết. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or NotFound if the expected transaction is not found or cannot be confirmed. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc NotFound nếu không tìm thấy giao dịch dự kiến hoặc không thể xác nhận. (VI)
    /// </returns>
    [HttpPost("{expectedTransactionId:guid}/confirm")]
    public async Task<IActionResult> ConfirmExpectedTransaction(Guid expectedTransactionId,
        [FromBody] ConfirmTransactionRequest request)
    {
        var result = await service.ConfirmExpectedTransactionAsync(expectedTransactionId, request.ActualTransactionId);
        if (!result)
            return NotFound("Expected transaction not found or cannot be confirmed");

        return Ok(new { success = true, message = "Expected transaction confirmed successfully" });
    }

    /// <summary>
    ///     Cancels an expected transaction by its ID. (EN)<br />
    ///     Hủy một giao dịch dự kiến dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="expectedTransactionId">
    ///     The ID of the expected transaction to cancel. (EN)<br />
    ///     ID của giao dịch dự kiến cần hủy. (VI)
    /// </param>
    /// <param name="request">
    ///     The request containing the reason for cancellation. (EN)<br />
    ///     Request chứa lý do hủy. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or NotFound if the expected transaction is not found or cannot be cancelled. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc NotFound nếu không tìm thấy giao dịch dự kiến hoặc không thể hủy. (VI)
    /// </returns>
    [HttpPost("{expectedTransactionId:guid}/cancel")]
    public async Task<IActionResult> CancelExpectedTransaction(Guid expectedTransactionId,
        [FromBody] CancelTransactionRequest request)
    {
        var result = await service.CancelExpectedTransactionAsync(expectedTransactionId, request.Reason);
        if (!result)
            return NotFound("Expected transaction not found or cannot be cancelled");

        return Ok(new { success = true, message = "Expected transaction cancelled successfully" });
    }

    /// <summary>
    ///     Adjusts the amount of an expected transaction by its ID. (EN)<br />
    ///     Điều chỉnh số tiền của một giao dịch dự kiến dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="expectedTransactionId">
    ///     The ID of the expected transaction to adjust. (EN)<br />
    ///     ID của giao dịch dự kiến cần điều chỉnh. (VI)
    /// </param>
    /// <param name="request">
    ///     The request containing the new amount and reason for adjustment. (EN)<br />
    ///     Request chứa số tiền mới và lý do điều chỉnh. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, or NotFound if the expected transaction is not found or cannot be adjusted. (EN)<br />
    ///     Kết quả Ok nếu thành công, hoặc NotFound nếu không tìm thấy giao dịch dự kiến hoặc không thể điều chỉnh. (VI)
    /// </returns>
    [HttpPost("{expectedTransactionId:guid}/adjust")]
    public async Task<IActionResult> AdjustExpectedTransaction(Guid expectedTransactionId,
        [FromBody] AdjustTransactionRequest request)
    {
        var result =
            await service.AdjustExpectedTransactionAsync(expectedTransactionId, request.NewAmount, request.Reason);
        if (!result)
            return NotFound("Expected transaction not found or cannot be adjusted");

        return Ok(new { success = true, message = "Expected transaction adjusted successfully" });
    }

    /// <summary>
    ///     Gets the cash flow forecast for a user within a specified date range. (EN)<br />
    ///     Lấy dự báo dòng tiền cho người dùng trong một khoảng thời gian được chỉ định. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <param name="startDate">
    ///     The start date of the forecast range. (EN)<br />
    ///     Ngày bắt đầu của khoảng thời gian dự báo. (VI)
    /// </param>
    /// <param name="endDate">
    ///     The end date of the forecast range. (EN)<br />
    ///     Ngày kết thúc của khoảng thời gian dự báo. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result with the cash flow forecast data if successful. (EN)<br />
    ///     Kết quả Ok kèm dữ liệu dự báo dòng tiền nếu thành công. (VI)
    /// </returns>
    [HttpGet("cash-flow-forecast/{userId:guid}")]
    public async Task<IActionResult> GetCashFlowForecast(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await service.GetCashFlowForecastAsync(userId, startDate, endDate);
        return Ok(new { cashFlowForecast = result, startDate, endDate });
    }

    /// <summary>
    ///     Gets the category-based forecast for a user within a specified date range. (EN)<br />
    ///     Lấy dự báo theo danh mục cho người dùng trong một khoảng thời gian được chỉ định. (VI)
    /// </summary>
    /// <param name="userId">
    ///     The ID of the user. (EN)<br />
    ///     ID của người dùng. (VI)
    /// </param>
    /// <param name="startDate">
    ///     The start date of the forecast range. (EN)<br />
    ///     Ngày bắt đầu của khoảng thời gian dự báo. (VI)
    /// </param>
    /// <param name="endDate">
    ///     The end date of the forecast range. (EN)<br />
    ///     Ngày kết thúc của khoảng thời gian dự báo. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result with the category forecast data if successful. (EN)<br />
    ///     Kết quả Ok kèm dữ liệu dự báo theo danh mục nếu thành công. (VI)
    /// </returns>
    [HttpGet("category-forecast/{userId:guid}")]
    public async Task<IActionResult> GetCategoryForecast(Guid userId, [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var result = await service.GetCategoryForecastAsync(userId, startDate, endDate);
        return Ok(new { categoryForecast = result, startDate, endDate });
    }
}