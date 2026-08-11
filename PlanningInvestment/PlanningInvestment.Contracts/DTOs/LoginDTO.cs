namespace CoreFinance.Contracts.DTOs;

/// <summary>
/// Represents a Data Transfer Object for user login information. (EN)<br/>
/// Đại diện cho Đối tượng truyền dữ liệu cho thông tin đăng nhập người dùng. (VI)
/// </summary>
public class LoginDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginDto"/> class. (EN)<br/>
    /// Khởi tạo một phiên bản mới của lớp <see cref="LoginDto"/>. (VI)
    /// </summary>
    public LoginDto()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginDto"/> class with username and password. (EN)<br/>
    /// Khởi tạo một phiên bản mới của lớp <see cref="LoginDto"/> với tên người dùng và mật khẩu. (VI)
    /// </summary>
    /// <param name="userName">
    /// The username. (EN)<br/>
    /// Tên người dùng. (VI)
    /// </param>
    /// <param name="password">
    /// The password. (EN)<br/>
    /// Mật khẩu. (VI)
    /// </param>
    public LoginDto(string? userName, string? password)
    {
        UserName = userName;
        Password = password;
    }

    /// <summary>
    /// Gets or sets the username. (EN)<br/>
    /// Lấy hoặc đặt tên người dùng. (VI)
    /// </summary>
    public string? UserName { get; set; }
    /// <summary>
    /// Gets or sets the password. (EN)<br/>
    /// Lấy hoặc đặt mật khẩu. (VI)
    /// </summary>
    public string? Password { get; set; }
}