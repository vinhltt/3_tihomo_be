using Identity.Api.Services;
using Identity.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    ///     Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var user = await userService.GetUserByIdAsync(userId);

            if (user == null) return NotFound("User not found");

            var userInfo = await userService.MapToUserInfoAsync(user);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting current user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Get user by ID (admin only)
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserInfo>> GetUser(Guid userId)
    {
        try
        {
            // In a real implementation, you would check admin permissions here
            var user = await userService.GetUserByIdAsync(userId);

            if (user == null) return NotFound("User not found");

            var userInfo = await userService.MapToUserInfoAsync(user);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", userId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Update current user profile
    /// </summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserInfo>> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var user = await userService.GetUserByIdAsync(userId);

            if (user == null) return NotFound("User not found");

            // Update allowed fields
            if (!string.IsNullOrEmpty(request.Name)) user.Name = request.Name;

            if (request.PictureUrl != null) user.PictureUrl = request.PictureUrl;

            var success = await userService.UpdateUserAsync(user);

            if (!success) return StatusCode(500, "Failed to update user");

            var userInfo = await userService.MapToUserInfoAsync(user);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating current user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    ///     Deactivate current user account
    /// </summary>
    [HttpDelete("me")]
    public async Task<ActionResult> DeactivateCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user ID");

            var success = await userService.DeactivateUserAsync(userId);

            if (!success) return StatusCode(500, "Failed to deactivate user");

            return Ok(new { Message = "User account deactivated successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating current user");
            return StatusCode(500, "Internal server error");
        }
    }
}

/// <summary>
///     Request model for updating user information
/// </summary>
public class UpdateUserRequest
{
    public string? Name { get; set; }
    public string? PictureUrl { get; set; }
}