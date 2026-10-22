using CoreFinance.Api.Controllers;
using CoreFinance.Application.DTOs.ExpectedTransaction;
using CoreFinance.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Contracts.Exceptions;

namespace CoreFinance.Api.Infrastructures.Filters;

/// <summary>
///     (EN) Preserves the expected-transaction API's local model-state and action-exception error contract.<br />
///     (VI) Giữ hợp đồng lỗi model-state và exception của action cục bộ cho API giao dịch dự kiến.
/// </summary>
/// <remarks>
///     (EN) Applied only to ExpectedTransactionController before automatic model-state validation; authorization remains
///     outside this boundary. Handled exceptions and requested cancellation retain their existing behavior.<br />
///     (VI) Chỉ áp dụng cho ExpectedTransactionController trước kiểm tra model-state tự động; phân quyền vẫn nằm ngoài
///     ranh giới này. Exception đã xử lý và yêu cầu hủy giữ hành vi hiện tại.
/// </remarks>
/// <param name="logger">(EN) Uses the existing controller log category.<br />(VI) Dùng category log hiện có của controller.</param>
public sealed class ExpectedTransactionActionFilter(ILogger<ExpectedTransactionController> logger) : IAsyncActionFilter
{
    /// <summary>
    ///     (EN) Short-circuits invalid model state and maps only unhandled action exceptions, preserving cancellation.<br />
    ///     (VI) Dừng sớm khi model-state không hợp lệ, chỉ ánh xạ exception action chưa xử lý và giữ hành vi hủy.
    /// </summary>
    /// <param name="context">(EN) The current action and HTTP context.<br />(VI) Ngữ cảnh action và HTTP hiện tại.</param>
    /// <param name="next">(EN) Executes the remaining action pipeline.<br />(VI) Chạy phần pipeline action còn lại.</param>
    /// <returns>(EN) Completes after short-circuiting or action execution.<br />(VI) Hoàn tất sau khi dừng sớm hoặc thực thi action.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = Error(StatusCodes.Status400BadRequest, CoreFinanceErrorCodes.InvalidRequest, CoreFinanceMessages.MissingOrInvalidFields);
            return;
        }

        var executed = await next();
        if (executed.Exception == null || executed.ExceptionHandled) return;
        if (executed.Exception is OperationCanceledException && context.HttpContext.RequestAborted.IsCancellationRequested) return;
        executed.Result = MapException(executed.Exception, context.HttpContext);
        executed.ExceptionHandled = true;
    }

    private ActionResult MapException(Exception exception, HttpContext httpContext)
    {
        switch (exception)
        {
            case ValidationException validation:
                return Error(StatusCodes.Status400BadRequest, CoreFinanceErrorCodes.InvalidRequest, validation.Errors.FirstOrDefault()?.ErrorMessage ?? validation.Message);
            case EntityNotFoundException:
                return Error(StatusCodes.Status404NotFound, CoreFinanceErrorCodes.ResourceNotFound, CoreFinanceMessages.ExpectedTransactionOrResourceNotFound);
            case ExpectedTransactionConflictException conflict:
                return Error(StatusCodes.Status409Conflict, CoreFinanceErrorCodes.StateConflict, conflict.Message);
            case DbUpdateConcurrencyException:
                return Error(StatusCodes.Status409Conflict, CoreFinanceErrorCodes.ConcurrencyConflict, CoreFinanceMessages.PeriodChanged);
            case DbUpdateException { InnerException: PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_expected_actual_transaction" } }:
                return Error(StatusCodes.Status409Conflict, CoreFinanceErrorCodes.AlreadyLinked, CoreFinanceMessages.ActualTransactionAlreadyLinked);
            case DbUpdateException { InnerException: PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_expected_template_day" } }:
                return Error(StatusCodes.Status409Conflict, CoreFinanceErrorCodes.DuplicatePeriod, CoreFinanceMessages.DuplicatePeriod);
            case KeyNotFoundException:
            case ArgumentException:
            case FormatException:
            case OverflowException:
                return Error(StatusCodes.Status400BadRequest, CoreFinanceErrorCodes.InvalidRequest, CoreFinanceMessages.InvalidValues);
            case UnauthorizedAccessException:
                return Error(httpContext.User.Identities.Any(i => i.IsAuthenticated) ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized,
                    CoreFinanceErrorCodes.Unauthorized, CoreFinanceMessages.AuthenticatedOwnerRequired);
            default:
                logger.LogError(exception, "Expected transaction request failed");
                return Error(StatusCodes.Status500InternalServerError, CoreFinanceErrorCodes.PersistenceError, CoreFinanceMessages.OperationCouldNotBeCompleted);
        }
    }

    private static ObjectResult Error(int status, string code, string message) => new(new { code, message }) { StatusCode = status };
}
