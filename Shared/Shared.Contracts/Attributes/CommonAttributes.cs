namespace Shared.Contracts.Attributes;

/// <summary>
/// Attribute to mark properties for auditing
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AuditableAttribute : Attribute
{
    public bool IsAuditable { get; set; } = true;
    
    public AuditableAttribute(bool isAuditable = true)
    {
        IsAuditable = isAuditable;
    }
}

/// <summary>
/// Attribute to mark entities for soft delete
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SoftDeleteAttribute : Attribute
{
    public bool IsSoftDelete { get; set; } = true;
    
    public SoftDeleteAttribute(bool isSoftDelete = true)
    {
        IsSoftDelete = isSoftDelete;
    }
}

/// <summary>
/// Attribute to specify table name prefix
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class TablePrefixAttribute : Attribute
{
    public string Prefix { get; set; }
    
    public TablePrefixAttribute(string prefix)
    {
        Prefix = prefix;
    }
}
