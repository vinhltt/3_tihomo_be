using CoreFinance.Application.Services.Base;
using Shared.EntityFramework.DTOs;
using Microsoft.AspNetCore.Mvc;
using Shared.EntityFramework.BaseEfModels;
using CoreFinance.Api.Extensions;
using CoreFinance.Application.Interfaces;

namespace CoreFinance.Api.Controllers.Base;

//[Authorize]
public abstract class CrudController<TEntity, TCreateRequest, TUpdateRequest,
    TViewModel, TKey>(
    ILogger logger,
    IBaseService<TEntity, TCreateRequest, TUpdateRequest, TViewModel, TKey>
        baseService
) : BaseController(logger)
    where TEntity : UserOwnedEntity<TKey>, new()
    where TCreateRequest : BaseCreateRequest, new()
    where TUpdateRequest : BaseUpdateRequest<TKey>, new()
    where TViewModel : BaseViewModel<TKey>, new()
{
    /// <summary>
    ///     Creates a new entity. (EN)<br />
    ///     Tạo một thực thể mới. (VI)
    /// </summary>
    /// <param name="request">
    ///     The request object containing data for the new entity. (EN)<br />
    ///     Đối tượng request chứa dữ liệu cho thực thể mới. (VI)
    /// </param>
    /// <returns>
    ///     The created entity if successful, or a status code indicating an error. (EN)<br />
    ///     Thực thể đã tạo nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPost]
    public virtual async Task<ActionResult> Post([FromForm] TCreateRequest? request)
    {
        if (null == request)
            return BadRequest();

        // Debug: Log the incoming request
        Logger.LogWarning("DEBUG: Incoming request type: {RequestType}", request.GetType().Name);
        Logger.LogWarning("DEBUG: Request content: {RequestContent}", System.Text.Json.JsonSerializer.Serialize(request));
        
        // Extract UserId from JWT claims and set it in the request if it supports IUserRequest
        if (request is IUserRequest userRequest)
        {
            Logger.LogWarning("DEBUG: User.Identity.IsAuthenticated = {IsAuthenticated}", User.Identity?.IsAuthenticated);
            Logger.LogWarning("DEBUG: User.Identity.Name = {Name}", User.Identity?.Name);
            Logger.LogWarning("DEBUG: User.Claims.Count = {ClaimsCount}", User.Claims.Count());
            
            // Debug: Log all available claims
            Logger.LogWarning("=== All JWT Claims ===");
            foreach (var claim in User.Claims)
            {
                Logger.LogWarning("Claim Type: '{ClaimType}', Value: '{ClaimValue}'", claim.Type, claim.Value);
            }
            Logger.LogWarning("=== End JWT Claims ===");

            var userId = User.GetUserId();
            if (userId.HasValue)
            {
                userRequest.UserId = userId;
                Logger.LogDebug("Setting UserId {UserId} for create request from JWT claims", userId);
            }
            else
            {
                Logger.LogWarning("Could not extract UserId from JWT claims for create request");
            }
        }

        var result = await baseService.CreateAsync(request);

        if (null == result)
            return StatusCode(StatusCodes.Status500InternalServerError);
        return Ok(result);
    }

    /// <summary>
    ///     Updates an existing entity. (EN)<br />
    ///     Cập nhật một thực thể hiện có. (VI)
    /// </summary>
    /// <param name="id">
    ///     The ID of the entity to update. (EN)<br />
    ///     ID của thực thể cần cập nhật. (VI)
    /// </param>
    /// <param name="request">
    ///     The request object containing updated data for the entity. (EN)<br />
    ///     Đối tượng request chứa dữ liệu cập nhật cho thực thể. (VI)
    /// </param>
    /// <returns>
    ///     The updated entity if successful, or a status code indicating an error. (EN)<br />
    ///     Thực thể đã cập nhật nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPut("{id}")]
    public virtual async Task<ActionResult> Put(TKey id, [FromForm] TUpdateRequest request)
    {
        if (!id!.Equals(request.Id))
            return BadRequest();
        try
        {
            return Ok(await baseService.UpdateAsync(id, request));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while updating the entity with ID {Id}", id);
            return StatusCode(500);
        }
    }

    /// <summary>
    ///     Soft deletes an entity by its ID. (EN)<br />
    ///     Xóa mềm một thực thể dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="id">
    ///     The ID of the entity to soft delete. (EN)<br />
    ///     ID của thực thể cần xóa mềm. (VI)
    /// </param>
    /// <returns>
    ///     An Ok result if successful, BadRequest if the ID is null, or NoContent if the entity was not found or not deleted.
    ///     (EN)<br />
    ///     Kết quả Ok nếu thành công, BadRequest nếu ID là null, hoặc NoContent nếu không tìm thấy thực thể hoặc không xóa
    ///     được. (VI)
    /// </returns>
    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete(TKey id)
    {
        try
        {
            var result = await baseService.DeleteSoftAsync(id);
            if (result > 0)
                return Ok();
        }
        catch (ArgumentNullException)
        {
            return BadRequest();
        }

        return NoContent();
    }

    /// <summary>
    ///     Gets a paginated list of entities based on filtering and sorting criteria. (EN)<br />
    ///     Lấy danh sách các thực thể có phân trang dựa trên tiêu chí lọc và sắp xếp. (VI)
    /// </summary>
    /// <param name="request">
    ///     The request object containing pagination, filtering, and sorting details. (EN)<br />
    ///     Đối tượng request chứa thông tin phân trang, lọc và sắp xếp. (VI)
    /// </param>
    /// <returns>
    ///     A paginated list of entities if successful, or a status code indicating an error. (EN)<br />
    ///     Danh sách thực thể có phân trang nếu thành công, hoặc mã trạng thái cho biết lỗi. (VI)
    /// </returns>
    [HttpPost("filter")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public abstract Task<ActionResult<IBasePaging<TViewModel>>>
        GetPagingAsync(FilterBodyRequest request);

    /// <summary>
    ///     Gets an entity by its ID. (EN)<br />
    ///     Lấy một thực thể dựa trên ID của nó. (VI)
    /// </summary>
    /// <param name="id">
    ///     The ID of the entity to get. (EN)<br />
    ///     ID của thực thể cần lấy. (VI)
    /// </param>
    /// <returns>
    ///     The entity if found, or a status code indicating an error or not found. (EN)<br />
    ///     Thực thể nếu tìm thấy, hoặc mã trạng thái cho biết lỗi hoặc không tìm thấy. (VI)
    /// </returns>
    [HttpGet("{id}")]
    public virtual async Task<ActionResult> GetById(TKey id)
    {
        var result = await baseService.GetByIdAsync(id);

        if (result != null)
            return Ok(result);
        return StatusCode(500);
    }
}