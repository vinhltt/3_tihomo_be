using System.Security.Claims;
using CoreFinance.Domain.BaseRepositories;
using CoreFinance.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace CoreFinance.Infrastructure;

/// <summary>
///     (EN) Scoped owner context. With an HTTP request present, only the authenticated NameIdentifier claim counts and a
///     background owner can never be set. Without HTTP, only an owner set once by the worker that owns the scope counts.<br />
///     (VI) Owner context theo scope. Khi có HTTP request, chỉ claim NameIdentifier đã xác thực được dùng và không thể đặt
///     owner nền. Khi không có HTTP, chỉ owner do worker sở hữu scope đặt một lần được dùng.
/// </summary>
public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private Guid? _backgroundUserId;

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null) return GetUserIdFromPrincipal(httpContext.User);

            return _backgroundUserId
                   ?? throw new UnauthorizedAccessException(
                       CoreFinanceMessages.NoUserContext);
        }
    }

    /// <inheritdoc />
    public string UserName => UserId.ToString();

    /// <summary>
    ///     (EN) Sets the owner for a background scope. Allowed once per scope and never inside an HTTP request.<br />
    ///     (VI) Đặt owner cho scope nền. Chỉ một lần mỗi scope và không bao giờ trong HTTP request.
    /// </summary>
    public void InitializeBackgroundOwner(Guid userId)
    {
        if (httpContextAccessor.HttpContext != null)
            throw new InvalidOperationException(CoreFinanceMessages.BackgroundOwnerInsideHttpRequest);
        if (_backgroundUserId != null)
            throw new InvalidOperationException(CoreFinanceMessages.BackgroundOwnerAlreadyInitialized);
        if (userId == Guid.Empty)
            throw new ArgumentException(CoreFinanceMessages.OwnerIdMustNotBeEmpty, nameof(userId));

        _backgroundUserId = userId;
    }

    private static Guid GetUserIdFromPrincipal(ClaimsPrincipal? principal)
    {
        var claim = principal?.Identities
            .Where(identity => identity.IsAuthenticated)
            .SelectMany(identity => identity.FindAll(ClaimTypes.NameIdentifier))
            .FirstOrDefault();

        if (claim != null && Guid.TryParse(claim.Value, out var userId) && userId != Guid.Empty)
            return userId;

        throw new UnauthorizedAccessException(CoreFinanceMessages.AuthenticatedNameIdentifierRequired);
    }
}
