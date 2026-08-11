//namespace CoreFinance.Api.Infrastructures.Middlewares;

//public class ExceptionHandlingMiddleware : IMiddleware
//{
//    private readonly ILogger _logger;
//    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
//    {
//        _logger = logger;
//    }

//    /// <summary>
//    /// Invokes the asynchronous.
//    /// </summary>
//    /// <param name="httpContext">The HTTP context.</param>
//    public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
//    {
//        try
//        {
//            Stopwatch stopwatch = null;
//            var slownessInMilliseconds = AppSettingsManager.ApiExecutionTimeSlownessInMilliseconds;

//            if (slownessInMilliseconds.HasValue)
//            {
//                stopwatch = new Stopwatch();
//                stopwatch.Start();
//            }

//            if (httpContext.Items != null)
//            {
//                httpContext.Items[External.Client.ApiConstants.HttpRequestHeaderItemsKeyInqueuedAt] = DateTime.Now;
//            }

//            await next(httpContext);

//            this.LogSlownessRequest(httpContext, stopwatch, slownessInMilliseconds);
//        }
//        catch (Exception exception)
//        {
//            await this.HandleApiExceptionAsync(httpContext, exception);
//        }
//    }/// <summary>
//     /// Logs the slowness request.
//     /// </summary>
//     /// <param name="httpContext">The HTTP context.</param>
//     /// <param name="stopwatch">The stopwatch.</param>
//     /// <param name="slownessInMilliseconds">The slowness in milliseconds.</param>
//    private void LogSlownessRequest(HttpContext httpContext, Stopwatch stopwatch, double? slownessInMilliseconds)
//    {
//        if (httpContext == null || stopwatch == null || !slownessInMilliseconds.HasValue)
//        {
//            return;
//        }

//        try
//        {
//            if (stopwatch.IsRunning)
//            {
//                stopwatch.Stop();
//            }

//            if (stopwatch.ElapsedMilliseconds < slownessInMilliseconds.Value)
//            {
//                return;
//            }

//            DateTime? inqueuedAt = null;
//            List<string> childRequestList = null;

//            var hasInqueuedAtInHttpItems = false;
//            var hasTrackingInfoInHttpItems = false;
//            var contextItems = httpContext?.Items;
//            if (contextItems != null)
//            {
//                if (contextItems.ContainsKey(External.Client.ApiConstants.HttpRequestHeaderItemsKeyInqueuedAt))
//                {
//                    hasInqueuedAtInHttpItems = true;
//                    inqueuedAt = httpContext.Items[External.Client.ApiConstants.HttpRequestHeaderItemsKeyInqueuedAt] as DateTime?;
//                }

//                if (contextItems.ContainsKey(External.Client.ApiConstants.HttpRequestHeaderItemsKeyTrackingInfo))
//                {
//                    hasTrackingInfoInHttpItems = true;
//                    childRequestList = httpContext.Items[External.Client.ApiConstants.HttpRequestHeaderItemsKeyTrackingInfo] as List<string>;
//                }
//            }

//            var inqueuedDts = inqueuedAt?.ToString("yyyy-MM-dd HH:mm:ss,fff");
//            var outqueuedDts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff");
//            var displayUrl = UriHelper.GetDisplayUrl(httpContext.Request);

//            var builder = new StringBuilder();
//            builder.AppendFormat("REQUEST: {0}. IN: {1}. OUT: {2}. DURATION: {3}", displayUrl, inqueuedDts, outqueuedDts, stopwatch.ElapsedMilliseconds);

//            if (childRequestList != null && childRequestList.Count > 0)
//            {
//                if (childRequestList.Count == 1)
//                {
//                    builder.AppendFormat(". ACTUAL REQUEST: {0}[{1}]", Environment.NewLine, string.Join(Environment.NewLine, childRequestList));
//                }
//                else
//                {
//                    builder.AppendFormat(". ACTUAL REQUESTS: {0}[{0}{1}{0}]", Environment.NewLine, string.Join(Environment.NewLine, childRequestList));
//                }
//            }

//            _logger.LogInformation(builder.ToString());

//            if (hasInqueuedAtInHttpItems)
//            {
//                contextItems.Remove(External.Client.ApiConstants.HttpRequestHeaderItemsKeyInqueuedAt);
//            }

//            if (hasTrackingInfoInHttpItems)
//            {
//                contextItems.Remove(External.Client.ApiConstants.HttpRequestHeaderItemsKeyTrackingInfo);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogInformation(ex.Message, ex);
//        }
//    }

//    /// <summary>
//    /// Handles the exception asynchronous.
//    /// </summary>
//    /// <param name="context">The context.</param>
//    /// <param name="exception">The exception.</param>
//    /// <returns></returns>
//    private Task HandleApiExceptionAsync(HttpContext context, Exception exception)
//    {
//        context.Response.ContentType = "application/json";
//        context.Response.StatusCode = (int)exception.StatusCode;
//        string responseContent = string.Empty;
//        if (!string.IsNullOrEmpty(exception.Message))
//        {
//            var response = new { message = Regex.Replace(exception.Message.Length > 1000 ? exception.Message.Substring(0, 1000) : exception.Message, @"<[^>]*>", string.Empty) };
//            responseContent = JsonSerializer.SerializeObject(response);
//        }

//        return context.Response.WriteAsync(responseContent);
//    }
//}

