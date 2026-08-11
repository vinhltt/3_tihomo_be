using System.Diagnostics;
using Castle.DynamicProxy;

namespace CoreFinance.Api.Infrastructures.Interceptors;

/// <summary>
///     Interceptor for monitoring and logging method execution time. (EN)<br />
///     Interceptor để giám sát và ghi nhật ký thời gian thực thi phương thức. (VI)
/// </summary>
public class MonitoringInterceptor(
    ILogger<MonitoringInterceptor> logger,
    IHttpContextAccessor httpContextAccessor)
    : AsyncTimingInterceptor
{
    private readonly ILogger _logger = logger;

    /// <summary>
    ///     Logs the completion of the method invocation with execution time. (EN)<br />
    ///     Ghi nhật ký hoàn thành việc gọi phương thức với thời gian thực thi. (VI)
    /// </summary>
    /// <param name="invocation">The method invocation being timed.</param>
    /// <param name="stopwatch">The stopwatch measuring the execution time.</param>
    protected override void CompletedTiming(IInvocation invocation, Stopwatch stopwatch)
    {
        var requestCtx = InitRequest();
        _logger.LogInformation(
            "[PERF] - RequestId - [{RequestCtxItem1}] - Method {ToStringInvocation} completed in {StopwatchElapsedMilliseconds}ms", requestCtx.Item1, ToStringInvocation(invocation), stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    ///     Logs the start of the method invocation. (EN)<br />
    ///     Ghi nhật ký bắt đầu việc gọi phương thức. (VI)
    /// </summary>
    /// <param name="invocation">The method invocation being timed.</param>
    protected override void StartingTiming(IInvocation invocation)
    {
        var requestCtx = InitRequest();
        _logger.LogInformation(
            "[PERF] - RequestId - [{RequestCtxItem1}] - Method {ToStringInvocation} invoked!", requestCtx.Item1, ToStringInvocation(invocation));
    }

    /// <summary>
    ///     Initializes request context information, including start time. (EN)<br />
    ///     Khởi tạo thông tin ngữ cảnh yêu cầu, bao gồm thời gian bắt đầu. (VI)
    /// </summary>
    /// <returns>A tuple containing the request trace identifier and start time.</returns>
    private Tuple<string?, DateTime> InitRequest()
    {
        var startTime = DateTime.UtcNow;
        var request = httpContextAccessor.HttpContext;
        try
        {
            if (request != null)
            {
                if (request.Items.TryGetValue("_RequestStartedAt", out var item))
                    startTime = (DateTime)item!;
                else
                    request.Items["_RequestStartedAt"] = startTime;
            }
        }
        catch
        {
            // ignored
        }

        return Tuple.Create(request?.TraceIdentifier, startTime);
    }

    /// <summary>
    ///     Returns a string representation of the method invocation target. (EN)<br />
    ///     Trả về biểu diễn chuỗi của mục tiêu gọi phương thức. (VI)
    /// </summary>
    /// <param name="invocation">The method invocation.</param>
    /// <returns>A string representing the method target.</returns>
    private string ToStringInvocation(IInvocation invocation)
    {
        return invocation.MethodInvocationTarget != null
            ? $"{invocation.MethodInvocationTarget.ReflectedType?.FullName}.{invocation.MethodInvocationTarget.Name}"
            : string.Empty;
    }
}