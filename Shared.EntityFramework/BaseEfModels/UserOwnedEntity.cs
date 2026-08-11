namespace Shared.EntityFramework.BaseEfModels;

/// <summary>
///     Base entity class for entities that are owned by a specific user (EN)<br/>
///     Lớp thực thể cơ sở cho các thực thể thuộc sở hữu của người dùng cụ thể (VI)
/// </summary>
/// <typeparam name="TKey">
///     The type of the entity's primary key (EN)<br/>
///     Kiểu của khóa chính của thực thể (VI)
/// </typeparam>
public abstract class UserOwnedEntity<TKey> : BaseEntity<TKey>
{
    /// <summary>
    ///     Gets or sets the ID of the user who owns this entity (EN)<br/>
    ///     Lấy hoặc đặt ID của người dùng sở hữu thực thể này (VI)
    /// </summary>
    /// <remarks>
    ///     This property is used to implement multi-tenant data isolation where each user 
    ///     only has access to their own data (EN)<br/>
    ///     Thuộc tính này được sử dụng để triển khai cách ly dữ liệu đa người thuê 
    ///     nơi mỗi người dùng chỉ có quyền truy cập vào dữ liệu của riêng họ (VI)
    /// </remarks>
    public Guid? UserId { get; set; }
    
    /// <summary>
    ///     Sets default values for creation properties including user ownership (EN)<br/>
    ///     Đặt các giá trị mặc định cho các thuộc tính tạo bao gồm quyền sở hữu người dùng (VI)
    /// </summary>
    /// <param name="createBy">The user who created the entity</param>
    /// <param name="userId">The ID of the user who owns this entity</param>
    /// <returns>The updated entity instance</returns>
    public virtual UserOwnedEntity<TKey> SetDefaultValueWithUser(string createBy, Guid? userId)
    {
        SetDefaultValue(createBy);
        UserId = userId;
        return this;
    }
}