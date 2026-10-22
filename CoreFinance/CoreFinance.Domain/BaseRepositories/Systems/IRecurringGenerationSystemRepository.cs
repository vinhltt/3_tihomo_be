namespace CoreFinance.Domain.BaseRepositories.Systems;

/// <summary>
///     (EN) Cross-owner catalogue only for the system auto-run worker. Returns only owner IDs and reads no financial
///     fields; owner-scoped template and expected-transaction reads go through the repositories.<br />
///     (VI) Danh mục chéo owner chỉ dành cho worker tự chạy của hệ thống. Chỉ trả về ID owner và không đọc trường tài
///     chính; các truy vấn mẫu và giao dịch dự kiến theo owner đi qua repository.
/// </summary>
public interface IRecurringGenerationSystemRepository
{
    /// <summary>
    ///     (EN) Cross-owner query only for the system auto-run worker. Returns only distinct owner IDs with at least one
    ///     active auto-generate template and reads no financial fields.<br />
    ///     (VI) Truy vấn chéo owner chỉ dành cho worker tự chạy của hệ thống. Chỉ trả về ID owner không trùng có ít nhất
    ///     một mẫu đang hoạt động và tự sinh, không đọc trường tài chính.
    /// </summary>
    /// <param name="cancellationToken">(EN) Cancels the catalogue query.<br />(VI) Hủy truy vấn danh mục owner.</param>
    /// <returns>(EN) Distinct owner IDs only, without financial data.<br />(VI) Chỉ các ID owner không trùng, không có dữ liệu tài chính.</returns>
    /// <exception cref="OperationCanceledException">(EN) The query is cancelled.<br />(VI) Truy vấn bị hủy.</exception>
    Task<IReadOnlyList<Guid>> GetAutoGenerateOwnerIdsAsync(CancellationToken cancellationToken = default);
}
