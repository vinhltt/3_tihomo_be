namespace CoreFinance.Domain.BaseRepositories;

/// <summary>
///     (EN) Read-only owner identity for the current DI scope. HTTP scopes resolve it from the authenticated
///     NameIdentifier claim; background scopes only from an owner set by the worker that created the scope.<br />
///     (VI) Danh tính chủ sở hữu chỉ đọc của DI scope hiện tại. Scope HTTP lấy từ claim NameIdentifier đã xác thực;
///     scope nền chỉ lấy từ owner do worker tạo scope khởi tạo.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    ///     (EN) Owner ID. Throws <see cref="UnauthorizedAccessException" /> when no valid owner exists.<br />
    ///     (VI) ID chủ sở hữu. Ném <see cref="UnauthorizedAccessException" /> khi không có owner hợp lệ.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    ///     (EN) Audit user name (owner ID string). Same fail-closed rule as <see cref="UserId" />.<br />
    ///     (VI) Tên người dùng cho audit (chuỗi owner ID). Cùng quy tắc fail-closed như <see cref="UserId" />.
    /// </summary>
    string UserName { get; }
}
