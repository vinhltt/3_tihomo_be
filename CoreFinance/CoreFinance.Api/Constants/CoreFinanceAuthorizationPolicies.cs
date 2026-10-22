namespace CoreFinance.Api.Constants;

/// <summary>
///     (EN) Names of authorization policies applied locally to CoreFinance endpoints.<br />
///     (VI) Tên các chính sách phân quyền áp dụng cục bộ cho endpoint CoreFinance.
/// </summary>
public static class CoreFinanceAuthorizationPolicies
{
    /// <summary>
    ///     (EN) Requires an authenticated identity with a nonempty GUID NameIdentifier before owner-scoped actions run.<br />
    ///     (VI) Yêu cầu danh tính đã xác thực có NameIdentifier là GUID khác rỗng trước khi chạy action theo phạm vi owner.
    /// </summary>
    public const string Owner = "CoreFinanceOwner";
}
