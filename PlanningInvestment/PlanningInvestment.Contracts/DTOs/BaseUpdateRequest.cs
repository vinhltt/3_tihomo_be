namespace CoreFinance.Contracts.DTOs;

/// <summary>
/// Base abstract class for update request DTOs providing common properties. (EN)<br/>
/// Lớp trừu tượng cơ sở cho các DTO yêu cầu cập nhật cung cấp các thuộc tính chung. (VI)
/// </summary>
/// <typeparam name="TKey">The type of the unique identifier.</typeparam>
public abstract class BaseUpdateRequest<TKey> : BaseDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity to update. (EN)<br/>
    /// Lấy hoặc đặt định danh duy nhất cho thực thể cần cập nhật. (VI)
    /// </summary>
    public virtual TKey? Id { get; set; }
    /// <summary>
    /// Gets or sets a flag indicating if the entity should be soft-deleted (optional). (EN)<br/>
    /// Lấy hoặc đặt cờ cho biết thực thể có nên bị xóa mềm hay không (tùy chọn). (VI)
    /// </summary>
    public string? Deleted { get; set; }
}