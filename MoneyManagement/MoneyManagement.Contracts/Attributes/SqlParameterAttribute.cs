using System.Data;
using System.Runtime.CompilerServices;

namespace MoneyManagement.Contracts.Attributes;

/// <summary>
///     Represents an attribute used to specify SQL parameter details for a property. (EN)<br />
///     Đại diện cho một attribute được sử dụng để chỉ định chi tiết tham số SQL cho một thuộc tính. (VI)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SqlParameterAttribute(DbType type, [CallerMemberName] string name = "") : Attribute
{
    /// <summary>
    ///     Gets or sets the database type of the SQL parameter. (EN)<br />
    ///     Lấy hoặc thiết lập kiểu dữ liệu cơ sở dữ liệu của tham số SQL. (VI)
    /// </summary>
    public DbType DbType { get; set; } = type;

    private string Name { get; } = name;

    /// <summary>
    ///     Gets the name of the SQL parameter, prefixed with "@". (EN)<br />
    ///     Lấy tên của tham số SQL, được thêm tiền tố "@". (VI)
    /// </summary>
    public string ParameterName => "@" + Name;
}