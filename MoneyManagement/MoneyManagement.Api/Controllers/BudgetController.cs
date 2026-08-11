using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Application.DTOs.Budget;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Api.Controllers;

/// <summary>
///     Budget management controller for budget CRUD operations and tracking (EN)<br />
///     Controller quản lý ngân sách cho các thao tác CRUD và theo dõi ngân sách (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
    : ControllerBase
{
    private const string BudgetNotFoundMessage = "Budget with ID {0} not found or access denied";
    private const string ErrorOccurredMessage = "An error occurred while {0} the budget";

    /// <summary>
    ///     Gets the current user ID from authentication context (temporary implementation)
    /// </summary>
    /// <returns>User ID</returns>
    private Guid GetCurrentUserId()
    {
        // TODO: Replace with actual authentication implementation
        // This should extract user ID from JWT token or session
        return Guid.CreateVersion7(); // Temporary - replace with actual user ID from auth context
    }

    /// <summary>
    ///     Creates a new budget (EN)<br />
    ///     Tạo ngân sách mới (VI)
    /// </summary>
    /// <param name="request">Budget creation request</param>
    /// <returns>Created budget details</returns>
    /// [HttpPost]
    [ProducesResponseType(typeof(BudgetViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BudgetViewModel>> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Creating budget for user {UserId}", userId);

            var result = await budgetService.CreateBudgetAsync(request, userId);

            logger.LogInformation("Budget created successfully with ID {BudgetId}", result.Id);

            return CreatedAtAction(nameof(GetBudgetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating budget");
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "creating"));
        }
    }

    /// <summary>
    ///     Updates an existing budget (EN)<br />
    ///     Cập nhật ngân sách hiện có (VI)
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="request">Budget update request</param>
    /// <returns>Updated budget details</returns>
    /// [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BudgetViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BudgetViewModel>> UpdateBudget(Guid id, [FromBody] UpdateBudgetRequest request)
    {
        try
        {
            if (id != request.Id) return BadRequest("ID in URL does not match ID in request body");

            var userId = GetCurrentUserId();

            logger.LogInformation("Updating budget {BudgetId} for user {UserId}", id, userId);

            var result = await budgetService.UpdateBudgetAsync(request, userId);

            logger.LogInformation("Budget {BudgetId} updated successfully", id);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, BudgetNotFoundMessage, id);
            return NotFound(string.Format(BudgetNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating budget {BudgetId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "updating"));
        }
    }

    /// <summary>
    ///     Deletes a budget (EN)<br />
    ///     Xóa ngân sách (VI)
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteBudget(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Deleting budget {BudgetId} for user {UserId}", id, userId);

            var result = await budgetService.DeleteBudgetAsync(id, userId);

            if (!result)
            {
                logger.LogWarning(BudgetNotFoundMessage, id);
                return NotFound(string.Format(BudgetNotFoundMessage, id));
            }

            logger.LogInformation("Budget {BudgetId} deleted successfully", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting budget {BudgetId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "deleting"));
        }
    }

    /// <summary>
    ///     Gets a budget by ID (EN)<br />
    ///     Lấy ngân sách theo ID (VI)
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <returns>Budget details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BudgetViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BudgetViewModel>> GetBudgetById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting budget {BudgetId} for user {UserId}", id, userId);

            var result = await budgetService.GetBudgetByIdAsync(id, userId);

            if (result == null)
            {
                logger.LogWarning(BudgetNotFoundMessage, id);
                return NotFound(string.Format(BudgetNotFoundMessage, id));
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting budget {BudgetId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving"));
        }
    }

    /// <summary>
    ///     Gets all budgets for the current user (EN)<br />
    ///     Lấy tất cả ngân sách của người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of user's budgets</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetAllBudgets()
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting all budgets for user {UserId}", userId);

            var result = await budgetService.GetBudgetsByUserIdAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting budgets for user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving"));
        }
    }

    /// <summary>
    ///     Gets all active budgets for the current user (EN)<br />
    ///     Lấy tất cả ngân sách đang hoạt động của người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of active budgets</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetActiveBudgets()
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting active budgets for user {UserId}", userId);

            var result = await budgetService.GetActiveBudgetsByUserIdAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting active budgets for user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving active"));
        }
    }

    /// <summary>
    ///     Gets budgets by category (EN)<br />
    ///     Lấy ngân sách theo danh mục (VI)
    /// </summary>
    /// <param name="category">Budget category</param>
    /// <returns>List of budgets in the specified category</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetBudgetsByCategory(string category)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting budgets for category {Category} for user {UserId}", category, userId);

            var result = await budgetService.GetBudgetsByCategoryAsync(userId, category);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting budgets for category {Category}", category);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving by category"));
        }
    }

    /// <summary>
    ///     Gets budgets by period (EN)<br />
    ///     Lấy ngân sách theo chu kỳ (VI)
    /// </summary>
    /// <param name="period">Budget period</param>
    /// <returns>List of budgets for the specified period</returns>
    [HttpGet("period/{period}")]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetBudgetsByPeriod(BudgetPeriod period)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting budgets for period {Period} for user {UserId}", period, userId);

            var result = await budgetService.GetBudgetsByPeriodAsync(userId, period);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting budgets for period {Period}", period);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving by period"));
        }
    }

    /// <summary>
    ///     Updates budget spent amount (EN)<br />
    ///     Cập nhật số tiền đã chi tiêu của ngân sách (VI)
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="amount">Amount to add to spent amount</param>
    /// <returns>Updated budget details</returns>
    [HttpPatch("{id:guid}/spent-amount")]
    [ProducesResponseType(typeof(BudgetViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BudgetViewModel>> UpdateBudgetSpentAmount(Guid id, [FromBody] decimal amount)
    {
        try
        {
            if (amount < 0) return BadRequest("Amount must be non-negative");

            var userId = GetCurrentUserId();

            logger.LogInformation("Updating spent amount for budget {BudgetId} by {Amount} for user {UserId}", id,
                amount, userId);

            var result = await budgetService.UpdateBudgetSpentAmountAsync(id, amount, userId);

            logger.LogInformation("Budget {BudgetId} spent amount updated successfully", id);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, BudgetNotFoundMessage, id);
            return NotFound(string.Format(BudgetNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating spent amount for budget {BudgetId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "updating spent amount for"));
        }
    }

    /// <summary>
    ///     Gets budgets that have reached their alert threshold (EN)<br />
    ///     Lấy các ngân sách đã đạt ngưỡng cảnh báo (VI)
    /// </summary>
    /// <returns>List of budgets that reached alert threshold</returns>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetBudgetsReachedAlertThreshold()
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting budgets that reached alert threshold for user {UserId}", userId);

            var result = await budgetService.GetBudgetsReachedAlertThresholdAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting budgets that reached alert threshold");
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving alert"));
        }
    }

    /// <summary>
    ///     Gets budgets that are over budget (EN)<br />
    ///     Lấy các ngân sách đã vượt quá giới hạn (VI)
    /// </summary>
    /// <returns>List of over-budget budgets</returns>
    [HttpGet("over-budget")]
    [ProducesResponseType(typeof(List<BudgetViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<BudgetViewModel>>> GetOverBudgetBudgets()
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Getting over-budget budgets for user {UserId}", userId);

            var result = await budgetService.GetOverBudgetBudgetsAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting over-budget budgets");
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "retrieving over-budget"));
        }
    }

    /// <summary>
    ///     Changes budget status (EN)<br />
    ///     Thay đổi trạng thái ngân sách (VI)
    /// </summary>
    /// <param name="id">Budget ID</param>
    /// <param name="status">New budget status</param>
    /// <returns>Updated budget details</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(BudgetViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BudgetViewModel>> ChangeBudgetStatus(Guid id, [FromBody] BudgetStatus status)
    {
        try
        {
            var userId = GetCurrentUserId();

            logger.LogInformation("Changing status for budget {BudgetId} to {Status} for user {UserId}", id, status,
                userId);

            var result = await budgetService.ChangeBudgetStatusAsync(id, status, userId);

            logger.LogInformation("Budget {BudgetId} status changed to {Status} successfully", id, status);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, BudgetNotFoundMessage, id);
            return NotFound(string.Format(BudgetNotFoundMessage, id));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error changing status for budget {BudgetId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                string.Format(ErrorOccurredMessage, "changing status for"));
        }
    }
}