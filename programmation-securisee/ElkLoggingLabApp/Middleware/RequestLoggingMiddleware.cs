using System.Diagnostics;

namespace ElkLoggingLabApp.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString("N");
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString.ToString();
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault() ?? "unknown";
        var requestContentLength = context.Request.ContentLength ?? 0;

        _logger.LogInformation(
            "HTTP {Method} {Path}{Query} started | IP: {ClientIp} | UserAgent: {UserAgent} | RequestSize: {RequestSize} | CorrelationId: {CorrelationId}",
            method, path, queryString, clientIp, userAgent, requestContentLength, correlationId);

        try
        {
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;
            var responseContentLength = context.Response.ContentLength ?? 0;

            var logLevel = statusCode >= 500 ? LogLevel.Error
                : statusCode >= 400 ? LogLevel.Warning
                : LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP {Method} {Path} completed {StatusCode} in {Duration}ms | ResponseSize: {ResponseSize} | CorrelationId: {CorrelationId}",
                method, path, statusCode, duration, responseContentLength, correlationId);

            if (duration > 500)
            {
                _logger.LogWarning(
                    "SLOW REQUEST: {Method} {Path} took {Duration}ms | CorrelationId: {CorrelationId}",
                    method, path, duration, correlationId);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "HTTP {Method} {Path} FAILED after {Duration}ms | CorrelationId: {CorrelationId}",
                method, path, stopwatch.ElapsedMilliseconds, correlationId);
            throw;
        }
    }
}
