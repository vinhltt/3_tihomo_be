namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Base response class for API responses. (EN)<br />
///     Lớp response cơ sở cho các phản hồi API. (VI)
/// </summary>
/// <typeparam name="T">The type of the response body.</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseResponse{T}" /> class. (EN)<br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="BaseResponse{T}" />. (VI)
    /// </summary>
    public BaseResponse()
    {
        Body = default;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseResponse{T}" /> class with status code, message, and body. (EN)
    ///     <br />
    ///     Khởi tạo một phiên bản mới của lớp <see cref="BaseResponse{T}" /> với mã trạng thái, thông báo và nội dung. (VI)
    /// </summary>
    /// <param name="statusCode">The status code of the response.</param>
    /// <param name="message">The message of the response.</param>
    /// <param name="body">The body of the response.</param>
    public BaseResponse(int statusCode, string message, T? body = default)
    {
        StatusCode = statusCode;
        Message = message;
        Body = body;
    }

    /// <summary>
    ///     Gets or sets the status code of the response. (EN)<br />
    ///     Lấy hoặc đặt mã trạng thái của phản hồi. (VI)
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    ///     Gets or sets the body of the response. (EN)<br />
    ///     Lấy hoặc đặt nội dung của phản hồi. (VI)
    /// </summary>
    public T? Body { get; set; }

    /// <summary>
    ///     Gets or sets the message of the response. (EN)<br />
    ///     Lấy hoặc đặt thông báo của phản hồi. (VI)
    /// </summary>
    public string? Message { get; set; }
}