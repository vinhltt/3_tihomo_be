namespace ExcelApi.Models;

/// <summary>
///     Standard API response wrapper for Excel processing operations (EN)<br />
///     Wrapper response API chuẩn cho các thao tác xử lý Excel (VI)
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    ///     Indicates if the API call was successful (EN)<br />
    ///     Cho biết API call có thành công hay không (VI)
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    ///     The response data (EN)<br />
    ///     Dữ liệu response (VI)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    ///     Error message if the call failed (EN)<br />
    ///     Thông báo lỗi nếu call thất bại (VI)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    ///     Additional error details for debugging (EN)<br />
    ///     Chi tiết lỗi bổ sung để debug (VI)
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    ///     HTTP status code (EN)<br />
    ///     Mã trạng thái HTTP (VI)
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    ///     Response timestamp (EN)<br />
    ///     Thời gian response (VI)
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     Creates a successful response (EN)<br />
    ///     Tạo response thành công (VI)
    /// </summary>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = 200
        };
    }

    /// <summary>
    ///     Creates an error response (EN)<br />
    ///     Tạo response lỗi (VI)
    /// </summary>
    public static ApiResponse<T> Error(string errorMessage, int statusCode = 500, string? errorDetails = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails,
            StatusCode = statusCode
        };
    }

    /// <summary>
    ///     Creates a bad request response (EN)<br />
    ///     Tạo response bad request (VI)
    /// </summary>
    public static ApiResponse<T> BadRequest(string errorMessage)
    {
        return Error(errorMessage, 400);
    }

    /// <summary>
    ///     Creates a not found response (EN)<br />
    ///     Tạo response not found (VI)
    /// </summary>
    public static ApiResponse<T> NotFound(string errorMessage)
    {
        return Error(errorMessage, 404);
    }
}

/// <summary>
///     Non-generic API response for operations that don't return data (EN)<br />
///     API response không generic cho các thao tác không trả về dữ liệu (VI)
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    ///     Creates a successful response without data (EN)<br />
    ///     Tạo response thành công không có dữ liệu (VI)
    /// </summary>
    public static ApiResponse Ok()
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = 200
        };
    }

    /// <summary>
    ///     Creates a successful response with a message (EN)<br />
    ///     Tạo response thành công có message (VI)
    /// </summary>
    public static ApiResponse Ok(string message)
    {
        return new ApiResponse
        {
            Success = true,
            Data = new { Message = message },
            StatusCode = 200
        };
    }
}