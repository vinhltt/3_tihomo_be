using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningInvestment.Contracts.BaseEfModels;

/// <summary>
/// Base entity class providing common properties for database entities. (EN)<br/>
/// Lớp thực thể cơ sở cung cấp các thuộc tính chung cho các thực thể cơ sở dữ liệu. (VI)
/// </summary>
[Microsoft.EntityFrameworkCore.Index(nameof(Id))]
public abstract class BaseEntity<TKey> : BaseEntity
{
    /// <summary>
    /// Sets default values for creation properties. (EN)<br/>
    /// Đặt các giá trị mặc định cho các thuộc tính tạo. (VI)
    /// </summary>
    /// <param name="createBy">The user who created the entity.</param>
    /// <returns>The updated entity instance.</returns>
    public virtual BaseEntity<TKey> SetDefaultValue(string createBy)
    {
        CreateAt = DateTime.Now;
        UpdateAt = CreateAt;
        CreateBy = createBy;
        return this;
    }

    /// <summary>
    /// Sets values for update properties. (EN)<br/>
    /// Đặt các giá trị cho các thuộc tính cập nhật. (VI)
    /// </summary>
    /// <param name="updateBy">The user who updated the entity.</param>
    /// <returns>The updated entity instance.</returns>
    public virtual BaseEntity<TKey> SetValueUpdate(string updateBy)
    {
        UpdateAt = DateTime.Now;
        UpdateBy = updateBy;
        return this;
    }

    /// <summary>
    /// Gets or sets the unique identifier for the entity. (EN)<br/>
    /// Lấy hoặc đặt định danh duy nhất cho thực thể. (VI)
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Required]
    public virtual TKey? Id { get; set; }
}

public class BaseEntity
{
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