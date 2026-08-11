using Identity.Application.Common.Interfaces;
using Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Identity.Api.Controllers;

/// <summary>
/// Enhanced API Keys Controller with complete RESTful endpoints (EN)<br/>
/// Controller API Keys nâng cao với đầy đủ RESTful endpoints (VI)
/// </summary>
[ApiController]
[Route("api/v1/api-keys")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class EnhancedApiKeysController(
    IEnhancedApiKeyService apiKeyService,
    ILogger<EnhancedApiKeysController> logger) : ControllerBase
{
    #region Core CRUD Operations

    /// <summary>
    /// Create a simple API key (end users) (EN)<br/>
    /// Tạo API key đơn giản cho end users (VI)
    /// </summary>
    /// <param name="request">Simple API key creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created API key with security details</returns>
    [HttpPost("simple")]
    [ProducesResponseType(typeof(CreateApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateApiKeyResponse>> CreateSimpleApiKeyAsync(
        [FromBody] CreateSimpleApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get user ID from JWT claims  
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid User",
                    Detail = "User ID not found in token",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var response = await apiKeyService.CreateSimpleApiKeyAsync(userId, request, cancellationToken);

            logger.LogInformation("Simple API key {KeyId} created successfully for user {UserId}", 
                response.Id, userId);

            // Return 201 Created with response directly to avoid route matching issues
            Response.Headers.Location = $"/api/v1/api-keys/{response.Id}";
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating simple API key for user {UserId}", userIdClaim);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating the simple API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Create a new API key with enhanced security features (EN)<br/>
    /// Tạo API key mới với tính năng bảo mật nâng cao (VI)
    /// </summary>
    /// <param name="request">API key creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created API key with security details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateApiKeyResponse>> CreateApiKeyAsync(
        [FromBody] CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Get user ID from JWT claims  
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid User",
                    Detail = "User ID not found in token",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var response = await apiKeyService.CreateApiKeyAsync(userId, request, cancellationToken);

            logger.LogInformation("API key {KeyId} created successfully for user {UserId}", 
                response.Id, userId);

            // Return 201 Created with response directly to avoid route matching issues
            Response.Headers.Location = $"/api/v1/api-keys/{response.Id}";
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating API key for user {UserId}", userIdClaim);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get API key by ID with enhanced details (EN)<br/>
    /// Lấy API key theo ID với thông tin chi tiết nâng cao (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API key details</returns>
    [HttpGet("{apiKeyId:guid}")]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiKeyResponse>> GetApiKeyByIdAsync(
        [FromRoute] Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await apiKeyService.GetApiKeyByIdAsync(apiKeyId, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get user's API keys with filtering and pagination (EN)<br/>
    /// Lấy danh sách API key của user với lọc và phân trang (VI)
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of API keys with pagination</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListApiKeysResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ListApiKeysResponse>> GetUserApiKeysAsync(
        [FromQuery] ListApiKeysQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid User",
                    Detail = "User ID not found in token",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var response = await apiKeyService.GetUserApiKeysAsync(userId, query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user API keys");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving API keys",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update API key with enhanced properties (EN)<br/>
    /// Cập nhật API key với thuộc tính nâng cao (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated API key details</returns>
    [HttpPut("{apiKeyId:guid}")]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiKeyResponse>> UpdateApiKeyAsync(
        [FromRoute] Guid apiKeyId,
        [FromBody] UpdateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await apiKeyService.UpdateApiKeyAsync(apiKeyId, request, cancellationToken);
            
            logger.LogInformation("API key {ApiKeyId} updated successfully", apiKeyId);
            
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while updating the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Revoke API key (EN)<br/>
    /// Thu hồi API key (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpPost("{apiKeyId:guid}/revoke")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeApiKeyAsync(
        [FromRoute] Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await apiKeyService.RevokeApiKeyAsync(apiKeyId, cancellationToken);
            
            logger.LogInformation("API key {ApiKeyId} revoked successfully", apiKeyId);
            
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while revoking the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Delete API key permanently (EN)<br/>
    /// Xóa API key vĩnh viễn (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{apiKeyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteApiKeyAsync(
        [FromRoute] Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await apiKeyService.DeleteApiKeyAsync(apiKeyId, cancellationToken);
            
            logger.LogInformation("API key {ApiKeyId} deleted successfully", apiKeyId);
            
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while deleting the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #endregion

    #region Security & Validation

    /// <summary>
    /// Exchange API key for JWT token (EN)<br/>
    /// Trao đổi API key lấy JWT token (VI)
    /// </summary>
    /// <param name="apiKey">API key to exchange</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT token response</returns>
    [HttpPost("exchange")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiKeyExchangeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiKeyExchangeResponse>> ExchangeApiKeyAsync(
        [FromBody][Required] string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "API key is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Get client IP address
            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? 
                                  HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                                  HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                                  "unknown";

            var response = await apiKeyService.ExchangeApiKeyForJwtAsync(apiKey, clientIpAddress, cancellationToken);
            
            logger.LogInformation("Successfully exchanged API key for JWT token for user {UserId}", response.UserId);
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized API key exchange attempt: {Message}", ex.Message);
            return Unauthorized(new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error exchanging API key for JWT token");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while exchanging the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Verify API key with enhanced security checks (EN)<br/>
    /// Xác thực API key với kiểm tra bảo mật nâng cao (VI)
    /// </summary>
    /// <param name="apiKey">API key to verify</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Verification result</returns>
    [HttpPost("verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerifyApiKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VerifyApiKeyResponse>> VerifyApiKeyAsync(
        [FromBody][Required] string apiKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "API key is required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            // Get client IP address
            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? 
                                  HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                                  HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                                  "unknown";

            var response = await apiKeyService.VerifyApiKeyAsync(apiKey, clientIpAddress, cancellationToken);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying API key");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while verifying the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Rotate API key (generate new key, keep same settings) (EN)<br/>
    /// Xoay API key (tạo key mới, giữ nguyên cài đặt) (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New API key details</returns>
    [HttpPost("{apiKeyId:guid}/rotate")]
    [ProducesResponseType(typeof(RotateApiKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RotateApiKeyResponse>> RotateApiKeyAsync(
        [FromRoute] Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await apiKeyService.RotateApiKeyAsync(apiKeyId, cancellationToken);
            
            logger.LogInformation("API key {ApiKeyId} rotated successfully", apiKeyId);
            
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rotating API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while rotating the API key",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #endregion

    #region Usage Analytics

    /// <summary>
    /// Get usage analytics for API key (EN)<br/>
    /// Lấy phân tích sử dụng cho API key (VI)
    /// </summary>
    /// <param name="apiKeyId">API key identifier</param>
    /// <param name="request">Analytics query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Usage analytics data</returns>
    [HttpGet("{apiKeyId:guid}/analytics")]
    [ProducesResponseType(typeof(ApiKeyUsageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiKeyUsageResponse>> GetUsageAnalyticsAsync(
        [FromRoute] Guid apiKeyId,
        [FromQuery] UsageQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await apiKeyService.GetUsageAnalyticsAsync(apiKeyId, request, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"API key with ID {apiKeyId} not found",
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving analytics for API key {ApiKeyId}", apiKeyId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving analytics",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #endregion

    #region Health & Info

    /// <summary>
    /// Get API key health summary (EN)<br/>
    /// Lấy tóm tắt tình trạng API key (VI)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health summary</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetHealthSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid User",
                    Detail = "User ID not found in token",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var query = new ListApiKeysQuery { Limit = 100 };
            var apiKeys = await apiKeyService.GetUserApiKeysAsync(userId, query, cancellationToken);

            var summary = new
            {
                TotalKeys = apiKeys.Data.Count,
                ActiveKeys = apiKeys.Data.Count(k => k.IsActive),
                ExpiredKeys = apiKeys.Data.Count(k => k.IsExpired),
                RevokedKeys = apiKeys.Data.Count(k => k.IsRevoked),
                RateLimitedKeys = apiKeys.Data.Count(k => k.IsRateLimited),
                TotalUsage = apiKeys.Data.Sum(k => k.UsageCount),
                Timestamp = DateTime.UtcNow
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving API key health summary");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving health summary",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #endregion
} 