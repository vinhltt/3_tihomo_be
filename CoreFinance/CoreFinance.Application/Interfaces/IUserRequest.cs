namespace CoreFinance.Application.Interfaces;

/// <summary>
///     Interface for requests that contain a UserId property. (EN)<br/>
///     Interface cho các request có chứa thuộc tính UserId. (VI)
/// </summary>
public interface IUserRequest
{
    /// <summary>
    ///     The ID of the user making the request. (EN)<br/>
    ///     ID của người dùng thực hiện request. (VI)
    /// </summary>
    Guid? UserId { get; set; }
}