using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Application.DTOs.SharedExpense;
using MoneyManagement.Application.DTOs.SharedExpenseParticipant;
using MoneyManagement.Application.Interfaces;

namespace MoneyManagement.Api.Controllers;

/// <summary>
///     Controller for managing shared expenses (EN)<br />
///     Controller quản lý chi tiêu chung (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SharedExpenseController(
    ISharedExpenseService sharedExpenseService,
    ILogger<SharedExpenseController> logger)
    : ControllerBase
{
    private const string GenericErrorMessage = "An error occurred while processing your request";

    private readonly ILogger<SharedExpenseController> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly ISharedExpenseService _sharedExpenseService =
        sharedExpenseService ?? throw new ArgumentNullException(nameof(sharedExpenseService));

    #region SharedExpense Operations

    /// <summary>
    ///     Get all shared expenses for the current user (EN)<br />
    ///     Lấy tất cả chi tiêu chung của người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseResponseDto>>> GetAllSharedExpenses()
    {
        try
        {
            var sharedExpenses = await _sharedExpenseService.GetAllSharedExpensesAsync();
            return Ok(sharedExpenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all shared expenses");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get shared expense by ID (EN)<br />
    ///     Lấy chi tiêu chung theo ID (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung (VI)</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SharedExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseResponseDto>> GetSharedExpenseById(Guid id)
    {
        try
        {
            var sharedExpense = await _sharedExpenseService.GetSharedExpenseByIdAsync(id);
            if (sharedExpense == null) return NotFound($"Shared expense with ID {id} not found");
            return Ok(sharedExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Create a new shared expense (EN)<br />
    ///     Tạo chi tiêu chung mới (VI)
    /// </summary>
    /// <param name="createDto">Create shared expense DTO (EN)<br />DTO tạo chi tiêu chung (VI)</param>
    /// <returns>Created shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã tạo (VI)</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SharedExpenseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseResponseDto>> CreateSharedExpense(
        [FromBody] CreateSharedExpenseRequestDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sharedExpense = await _sharedExpenseService.CreateSharedExpenseAsync(createDto);
            return CreatedAtAction(nameof(GetSharedExpenseById), new { id = sharedExpense.Id }, sharedExpense);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating shared expense");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating shared expense");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Update an existing shared expense (EN)<br />
    ///     Cập nhật chi tiêu chung hiện có (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <param name="updateDto">Update shared expense DTO (EN)<br />DTO cập nhật chi tiêu chung (VI)</param>
    /// <returns>Updated shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã cập nhật (VI)</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SharedExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseResponseDto>> UpdateSharedExpense(Guid id,
        [FromBody] UpdateSharedExpenseRequestDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var sharedExpense = await _sharedExpenseService.UpdateSharedExpenseAsync(id, updateDto);
            return Ok(sharedExpense);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Shared expense with ID {SharedExpenseId} not found for update", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating shared expense with ID {SharedExpenseId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Delete a shared expense (EN)<br />
    ///     Xóa chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Success indicator (EN)<br />Chỉ báo thành công (VI)</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteSharedExpense(Guid id)
    {
        try
        {
            var result = await _sharedExpenseService.DeleteSharedExpenseAsync(id);
            if (!result) return NotFound($"Shared expense with ID {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get shared expenses by status (EN)<br />
    ///     Lấy chi tiêu chung theo trạng thái (VI)
    /// </summary>
    /// <param name="status">Shared expense status (EN)<br />Trạng thái chi tiêu chung (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    [HttpGet("status/{status:int}")]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseResponseDto>>> GetSharedExpensesByStatus(int status)
    {
        try
        {
            if (status < 1 || status > 4) return BadRequest("Status must be between 1 and 4");

            var sharedExpenses = await _sharedExpenseService.GetSharedExpensesByStatusAsync(status);
            return Ok(sharedExpenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting shared expenses by status {Status}", status);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get shared expenses by date range (EN)<br />
    ///     Lấy chi tiêu chung theo khoảng thời gian (VI)
    /// </summary>
    /// <param name="startDate">Start date (EN)<br />Ngày bắt đầu (VI)</param>
    /// <param name="endDate">End date (EN)<br />Ngày kết thúc (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    [HttpGet("date-range")]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseResponseDto>>> GetSharedExpensesByDateRange(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate > endDate) return BadRequest("Start date cannot be greater than end date");

            var sharedExpenses = await _sharedExpenseService.GetSharedExpensesByDateRangeAsync(startDate, endDate);
            return Ok(sharedExpenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting shared expenses by date range {StartDate} to {EndDate}",
                startDate, endDate);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get shared expenses by group name (EN)<br />
    ///     Lấy chi tiêu chung theo tên nhóm (VI)
    /// </summary>
    /// <param name="groupName">Group name (EN)<br />Tên nhóm (VI)</param>
    /// <returns>List of shared expense response DTOs (EN)<br />Danh sách DTO phản hồi chi tiêu chung (VI)</returns>
    [HttpGet("group/{groupName}")]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseResponseDto>>> GetSharedExpensesByGroup(string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName)) return BadRequest("Group name cannot be empty");

            var sharedExpenses = await _sharedExpenseService.GetSharedExpensesByGroupAsync(groupName);
            return Ok(sharedExpenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting shared expenses by group {GroupName}", groupName);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Mark shared expense as settled (EN)<br />
    ///     Đánh dấu chi tiêu chung đã thanh toán (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Updated shared expense response DTO (EN)<br />DTO phản hồi chi tiêu chung đã cập nhật (VI)</returns>
    [HttpPost("{id:guid}/settle")]
    [ProducesResponseType(typeof(SharedExpenseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseResponseDto>> MarkAsSettled(Guid id)
    {
        try
        {
            var sharedExpense = await _sharedExpenseService.MarkAsSettledAsync(id);
            return Ok(sharedExpense);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Shared expense with ID {SharedExpenseId} not found for settling", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while settling shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    #endregion

    #region SharedExpenseParticipant Operations

    /// <summary>
    ///     Get all participants for a shared expense (EN)<br />
    ///     Lấy tất cả người tham gia cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>List of participant response DTOs (EN)<br />Danh sách DTO phản hồi người tham gia (VI)</returns>
    [HttpGet("{id:guid}/participants")]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseParticipantResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseParticipantResponseDto>>> GetParticipants(Guid id)
    {
        try
        {
            var participants = await _sharedExpenseService.GetParticipantsBySharedExpenseIdAsync(id);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while getting participants for shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Add a participant to a shared expense (EN)<br />
    ///     Thêm người tham gia vào chi tiêu chung (VI)
    /// </summary>
    /// <param name="createDto">Create participant DTO (EN)<br />DTO tạo người tham gia (VI)</param>
    /// <returns>Created participant response DTO (EN)<br />DTO phản hồi người tham gia đã tạo (VI)</returns>
    [HttpPost("participants")]
    [ProducesResponseType(typeof(SharedExpenseParticipantResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseParticipantResponseDto>> AddParticipant(
        [FromBody] CreateSharedExpenseParticipantRequestDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var participant = await _sharedExpenseService.AddParticipantAsync(createDto);
            return CreatedAtAction(nameof(GetParticipants), new { id = createDto.SharedExpenseId }, participant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while adding participant");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding participant");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Update a participant's information (EN)<br />
    ///     Cập nhật thông tin người tham gia (VI)
    /// </summary>
    /// <param name="participantId">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <param name="updateDto">Update participant DTO (EN)<br />DTO cập nhật người tham gia (VI)</param>
    /// <returns>Updated participant response DTO (EN)<br />DTO phản hồi người tham gia đã cập nhật (VI)</returns>
    [HttpPut("participants/{participantId:guid}")]
    [ProducesResponseType(typeof(SharedExpenseParticipantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseParticipantResponseDto>> UpdateParticipant(Guid participantId,
        [FromBody] UpdateSharedExpenseParticipantRequestDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var participant = await _sharedExpenseService.UpdateParticipantAsync(participantId, updateDto);
            return Ok(participant);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Participant with ID {ParticipantId} not found for update", participantId);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating participant with ID {ParticipantId}",
                participantId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating participant with ID {ParticipantId}", participantId);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Remove a participant from a shared expense (EN)<br />
    ///     Xóa người tham gia khỏi chi tiêu chung (VI)
    /// </summary>
    /// <param name="participantId">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <returns>Success indicator (EN)<br />Chỉ báo thành công (VI)</returns>
    [HttpDelete("participants/{participantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveParticipant(Guid participantId)
    {
        try
        {
            var result = await _sharedExpenseService.RemoveParticipantAsync(participantId);
            if (!result) return NotFound($"Participant with ID {participantId} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing participant with ID {ParticipantId}", participantId);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Record a payment from a participant (EN)<br />
    ///     Ghi nhận thanh toán từ người tham gia (VI)
    /// </summary>
    /// <param name="participantId">Participant ID (EN)<br />ID người tham gia (VI)</param>
    /// <param name="amount">Payment amount (EN)<br />Số tiền thanh toán (VI)</param>
    /// <param name="paymentMethod">Payment method (EN)<br />Phương thức thanh toán (VI)</param>
    /// <returns>Updated participant response DTO (EN)<br />DTO phản hồi người tham gia đã cập nhật (VI)</returns>
    [HttpPost("participants/{participantId:guid}/payment")]
    [ProducesResponseType(typeof(SharedExpenseParticipantResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseParticipantResponseDto>> RecordPayment(Guid participantId,
        [FromQuery] decimal amount, [FromQuery] string? paymentMethod = null)
    {
        try
        {
            if (amount <= 0) return BadRequest("Payment amount must be greater than zero");

            var participant = await _sharedExpenseService.RecordPaymentAsync(participantId, amount, paymentMethod);
            return Ok(participant);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Participant with ID {ParticipantId} not found for payment recording",
                participantId);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while recording payment for participant with ID {ParticipantId}",
                participantId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording payment for participant with ID {ParticipantId}",
                participantId);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get unsettled participants for a shared expense (EN)<br />
    ///     Lấy người tham gia chưa thanh toán cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>
    ///     List of unsettled participant response DTOs (EN)<br />Danh sách DTO phản hồi người tham gia chưa thanh toán
    ///     (VI)
    /// </returns>
    [HttpGet("{id:guid}/participants/unsettled")]
    [ProducesResponseType(typeof(IEnumerable<SharedExpenseParticipantResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SharedExpenseParticipantResponseDto>>> GetUnsettledParticipants(Guid id)
    {
        try
        {
            var participants = await _sharedExpenseService.GetUnsettledParticipantsAsync(id);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while getting unsettled participants for shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    #endregion

    #region Calculation and Analysis

    /// <summary>
    ///     Calculate equal split amounts for a shared expense (EN)<br />
    ///     Tính số tiền chia đều cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Equal split calculation result (EN)<br />Kết quả tính chia đều (VI)</returns>
    [HttpGet("{id:guid}/equal-split")]
    [ProducesResponseType(typeof(EqualSplitCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EqualSplitCalculationDto>> CalculateEqualSplit(Guid id)
    {
        try
        {
            var calculation = await _sharedExpenseService.CalculateEqualSplitAsync(id);
            return Ok(calculation);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Shared expense with ID {SharedExpenseId} not found for equal split calculation",
                id);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Invalid operation while calculating equal split for shared expense with ID {SharedExpenseId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while calculating equal split for shared expense with ID {SharedExpenseId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get expense summary for a shared expense (EN)<br />
    ///     Lấy tóm tắt chi tiêu cho chi tiêu chung (VI)
    /// </summary>
    /// <param name="id">Shared expense ID (EN)<br />ID chi tiêu chung (VI)</param>
    /// <returns>Expense summary DTO (EN)<br />DTO tóm tắt chi tiêu (VI)</returns>
    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(SharedExpenseSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SharedExpenseSummaryDto>> GetExpenseSummary(Guid id)
    {
        try
        {
            var summary = await _sharedExpenseService.GetExpenseSummaryAsync(id);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Shared expense with ID {SharedExpenseId} not found for summary", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting summary for shared expense with ID {SharedExpenseId}",
                id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get user's shared expense statistics (EN)<br />
    ///     Lấy thống kê chi tiêu chung của người dùng (VI)
    /// </summary>
    /// <returns>User statistics DTO (EN)<br />DTO thống kê người dùng (VI)</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(UserSharedExpenseStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserSharedExpenseStatsDto>> GetUserStatistics()
    {
        try
        {
            var statistics = await _sharedExpenseService.GetUserStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user shared expense statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    #endregion
}