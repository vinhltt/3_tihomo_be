namespace CoreFinance.Contracts.DTOs;

/// <summary>
/// Base abstract class for view models providing common properties. (EN)<br/>
/// Lớp trừu tượng cơ sở cho các view model cung cấp các thuộc tính chung. (VI)
/// </summary>
/// <typeparam name="TKey">The type of the unique identifier.</typeparam>
public abstract class BaseViewModel<TKey> : BaseDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity. (EN)<br/>
    /// Lấy hoặc đặt định danh duy nhất cho thực thể. (VI)
    /// </summary>
    public virtual TKey? Id { get; set; }
    /// <summary>
    /// Gets or sets the creation timestamp of the entity. (EN)<br/>
    /// Lấy hoặc đặt dấu thời gian tạo của thực thể. (VI)
    /// </summary>
    public DateTime? CreateAt { get; set; }
    /// <summary>
    /// Gets or sets the last update timestamp of the entity. (EN)<br/>
    /// Lấy hoặc đặt dấu thời gian cập nhật cuối cùng của thực thể. (VI)
    /// </summary>
    public DateTime? UpdateAt { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who created the entity. (EN)<br/>
    /// Lấy hoặc đặt định danh của người dùng đã tạo thực thể. (VI)
    /// </summary>
    public string? CreateBy { get; set; }
    /// <summary>
    /// Gets or sets the identifier of the user who last updated the entity. (EN)<br/>
    /// Lấy hoặc đặt định danh của người dùng đã cập nhật thực thể lần cuối. (VI)
    /// </summary>
    public string? UpdateBy { get; set; }
    /// <summary>
    /// Gets or sets a flag indicating if the entity has been soft-deleted (optional). (EN)<br/>
    /// Lấy hoặc đặt cờ cho biết thực thể đã bị xóa mềm hay chưa (tùy chọn). (VI)
    /// </summary>
    public string? Deleted { get; set; }
}