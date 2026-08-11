using System.Diagnostics;
using Microsoft.IO;
using Shared.Contracts.Utilities;

namespace CoreFinance.Api.Infrastructures.Middlewares;

public class RequestLogging
{
    public string? Key { get; set; }
    public string? RequestId { get; set; }
    public string? Schema { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public string? RequestBody { get; set; }
}

/// <summary>
///     The Exception Handling Middleware.
/// </summary>
public class PerformanceLoggingMiddleware(
    ILogger<PerformanceLoggingMiddleware> logger,
    RequestDelegate next)
{
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager = new();

    /// <summary>
    ///     Invokes the asynchronous.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Validate parameters to prevent null dereference
        ArgumentNullException.ThrowIfNull(context);

        context.Request.EnableBuffering();
        await using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);
        var formatRequest = new RequestLogging
        {
            Key = "PERF",
            RequestId = context.TraceIdentifier,
            Schema = context.Request.Scheme,
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            RequestBody = ReadStreamInChunks(requestStream)
        };
        logger.LogInformation(formatRequest.TryParseToString());

        context.Request.Body.Position = 0;

        logger.LogInformation(
            "[PERF] - RequestId - [{ContextTraceIdentifier}] - Method {RequestPath} started", context.TraceIdentifier, context.Request.Path);

        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();
        logger.LogInformation(
            "[PERF] - RequestId - [{ContextTraceIdentifier}] - Method {RequestPath} - In [{StopwatchElapsedMilliseconds}] milliseconds completed.", context.TraceIdentifier, context.Request.Path, stopwatch.ElapsedMilliseconds);
    }

    private static string ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream);
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        do
        {
            readChunkLength = reader.ReadBlock(readChunk,
                0,
                readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);

        return textWriter.ToString();
    }
}