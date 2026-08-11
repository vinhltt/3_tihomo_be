namespace Identity.Domain.Dtos.Common;

public record BaseFilterRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "asc";
}

public record PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}

public record ErrorResponse(
    string Message,
    string? Details = null,
    Dictionary<string, string[]>? ValidationErrors = null);

public record SuccessResponse(string Message);

public record ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
    public string? ErrorDetails { get; init; }
}