namespace Shared.Contracts.Enums;

/// <summary>
/// Sort direction enumeration
/// </summary>
public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}

/// <summary>
/// Entity status enumeration
/// </summary>
public enum EntityStatus
{
    Active = 0,
    Inactive = 1,
    Deleted = 2,
    Archived = 3
}

/// <summary>
/// Operation result status
/// </summary>
public enum OperationStatus
{
    Success = 0,
    Failed = 1,
    Warning = 2,
    Info = 3
}

/// <summary>
/// Log level enumeration
/// </summary>
public enum LogLevel
{
    Debug = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}

/// <summary>
/// Data type enumeration for filters
/// </summary>
public enum FilterDataType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Enum = 4
}

/// <summary>
/// Filter operator enumeration
/// </summary>
public enum FilterOperator
{
    Equal = 0,
    NotEqual = 1,
    GreaterThan = 2,
    GreaterThanOrEqual = 3,
    LessThan = 4,
    LessThanOrEqual = 5,
    Contains = 6,
    StartsWith = 7,
    EndsWith = 8,
    In = 9,
    NotIn = 10
}
