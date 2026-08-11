using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Application.DTOs.Jar;
using MoneyManagement.Application.Interfaces;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Api.Controllers;

/// <summary>
///     Controller for managing jars (6 Jars methodology) (EN)<br />
///     Controller quản lý các jar (phương pháp 6 Jar) (VI)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JarController(IJarService jarService, ILogger<JarController> logger) : ControllerBase
{
    private const string GenericErrorMessage = "An error occurred while processing your request";
    private readonly IJarService _jarService = jarService ?? throw new ArgumentNullException(nameof(jarService));
    private readonly ILogger<JarController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    ///     Get all jars for the current user (EN)<br />
    ///     Lấy tất cả jar của người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of jar response DTOs (EN)<br />Danh sách DTO phản hồi jar (VI)</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JarResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<JarResponseDto>>> GetAllJars()
    {
        try
        {
            var jars = await _jarService.GetAllJarsAsync();
            return Ok(jars);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all jars");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get jar by ID (EN)<br />
    ///     Lấy jar theo ID (VI)
    /// </summary>
    /// <param name="id">Jar ID (EN)<br />ID jar (VI)</param>
    /// <returns>Jar response DTO (EN)<br />DTO phản hồi jar (VI)</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> GetJarById(Guid id)
    {
        try
        {
            var jar = await _jarService.GetJarByIdAsync(id);
            if (jar == null) return NotFound($"Jar with ID {id} not found");
            return Ok(jar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting jar with ID {JarId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get jar by type (EN)<br />
    ///     Lấy jar theo loại (VI)
    /// </summary>
    /// <param name="jarType">Jar type (EN)<br />Loại jar (VI)</param>
    /// <returns>Jar response DTO (EN)<br />DTO phản hồi jar (VI)</returns>
    [HttpGet("type/{jarType}")]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> GetJarByType(JarType jarType)
    {
        try
        {
            var jar = await _jarService.GetJarByTypeAsync(jarType);
            if (jar == null) return NotFound($"Jar with type {jarType} not found");
            return Ok(jar);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting jar with type {JarType}", jarType);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Create a new jar (EN)<br />
    ///     Tạo jar mới (VI)
    /// </summary>
    /// <param name="createDto">Create jar DTO (EN)<br />DTO tạo jar (VI)</param>
    /// <returns>Created jar response DTO (EN)<br />DTO phản hồi jar đã tạo (VI)</returns>
    [HttpPost]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> CreateJar([FromBody] CreateJarRequestDto createDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var jar = await _jarService.CreateJarAsync(createDto);
            return CreatedAtAction(nameof(GetJarById), new { id = jar.Id }, jar);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating jar");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating jar");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Update an existing jar (EN)<br />
    ///     Cập nhật jar hiện có (VI)
    /// </summary>
    /// <param name="id">Jar ID (EN)<br />ID jar (VI)</param>
    /// <param name="updateDto">Update jar DTO (EN)<br />DTO cập nhật jar (VI)</param>
    /// <returns>Updated jar response DTO (EN)<br />DTO phản hồi jar đã cập nhật (VI)</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> UpdateJar(Guid id, [FromBody] UpdateJarRequestDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var jar = await _jarService.UpdateJarAsync(id, updateDto);
            return Ok(jar);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Jar with ID {JarId} not found for update", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating jar with ID {JarId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Delete a jar (EN)<br />
    ///     Xóa jar (VI)
    /// </summary>
    /// <param name="id">Jar ID (EN)<br />ID jar (VI)</param>
    /// <returns>Success indicator (EN)<br />Chỉ báo thành công (VI)</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteJar(Guid id)
    {
        try
        {
            var result = await _jarService.DeleteJarAsync(id);
            if (!result) return NotFound($"Jar with ID {id} not found");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting jar with ID {JarId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Initialize default jars for the current user (EN)<br />
    ///     Khởi tạo jar mặc định cho người dùng hiện tại (VI)
    /// </summary>
    /// <returns>List of created jar response DTOs (EN)<br />Danh sách DTO phản hồi jar đã tạo (VI)</returns>
    [HttpPost("initialize")]
    [ProducesResponseType(typeof(IEnumerable<JarResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<JarResponseDto>>> InitializeDefaultJars()
    {
        try
        {
            var jars = await _jarService.InitializeDefaultJarsAsync();
            return Created("/api/jar", jars);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while initializing default jars");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while initializing default jars");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Add money to a jar (EN)<br />
    ///     Thêm tiền vào jar (VI)
    /// </summary>
    /// <param name="id">Jar ID (EN)<br />ID jar (VI)</param>
    /// <param name="addMoneyDto">Add money DTO (EN)<br />DTO thêm tiền (VI)</param>
    /// <returns>Updated jar response DTO (EN)<br />DTO phản hồi jar đã cập nhật (VI)</returns>
    [HttpPost("{id:guid}/add-money")]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> AddMoney(Guid id, [FromBody] AddMoneyToJarRequestDto addMoneyDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var jar = await _jarService.AddMoneyToJarAsync(id, addMoneyDto);
            return Ok(jar);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Jar with ID {JarId} not found for adding money", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while adding money to jar with ID {JarId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding money to jar with ID {JarId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Withdraw money from a jar (EN)<br />
    ///     Rút tiền từ jar (VI)
    /// </summary>
    /// <param name="id">Jar ID (EN)<br />ID jar (VI)</param>
    /// <param name="withdrawDto">Withdraw money DTO (EN)<br />DTO rút tiền (VI)</param>
    /// <returns>Updated jar response DTO (EN)<br />DTO phản hồi jar đã cập nhật (VI)</returns>
    [HttpPost("{id:guid}/withdraw")]
    [ProducesResponseType(typeof(JarResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarResponseDto>> WithdrawMoney(Guid id,
        [FromBody] WithdrawFromJarRequestDto withdrawDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var jar = await _jarService.WithdrawFromJarAsync(id, withdrawDto);
            return Ok(jar);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Jar with ID {JarId} not found for withdrawal", id);
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while withdrawing from jar with ID {JarId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while withdrawing from jar with ID {JarId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Transfer money between jars (EN)<br />
    ///     Chuyển tiền giữa các jar (VI)
    /// </summary>
    /// <param name="transferDto">Transfer money DTO (EN)<br />DTO chuyển tiền (VI)</param>
    /// <returns>Transfer result DTO (EN)<br />DTO kết quả chuyển (VI)</returns>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransferResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransferResultDto>> TransferMoney(
        [FromBody] TransferBetweenJarsRequestDto transferDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _jarService.TransferBetweenJarsAsync(transferDto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Jar not found for transfer operation");
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for transfer operation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while transferring money between jars");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Distribute income across jars using 6 Jars methodology (EN)<br />
    ///     Phân bổ thu nhập qua các jar theo phương pháp 6 Jar (VI)
    /// </summary>
    /// <param name="distributeDto">Distribute income DTO (EN)<br />DTO phân bổ thu nhập (VI)</param>
    /// <returns>Distribution result DTO (EN)<br />DTO kết quả phân bổ (VI)</returns>
    [HttpPost("distribute")]
    [ProducesResponseType(typeof(DistributionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DistributionResultDto>> DistributeIncome(
        [FromBody] DistributeIncomeRequestDto distributeDto)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _jarService.DistributeIncomeAsync(distributeDto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for income distribution");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while distributing income");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }

    /// <summary>
    ///     Get jar allocation summary (EN)<br />
    ///     Lấy tóm tắt phân bổ jar (VI)
    /// </summary>
    /// <returns>Jar allocation summary DTO (EN)<br />DTO tóm tắt phân bổ jar (VI)</returns>
    [HttpGet("allocation-summary")]
    [ProducesResponseType(typeof(JarAllocationSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<JarAllocationSummaryDto>> GetAllocationSummary()
    {
        try
        {
            var summary = await _jarService.GetJarAllocationSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting jar allocation summary");
            return StatusCode(StatusCodes.Status500InternalServerError, GenericErrorMessage);
        }
    }
}