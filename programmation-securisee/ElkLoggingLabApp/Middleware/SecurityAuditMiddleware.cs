using System.Text.RegularExpressions;

namespace ElkLoggingLabApp.Middleware;

public class SecurityAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityAuditMiddleware> _logger;

    private static readonly Regex[] SqlInjectionPatterns =
    [
        new(@"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|CREATE|EXEC)\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(--|;|'|\""|\/\*|\*\/)", RegexOptions.Compiled),
        new(@"(\bOR\b\s+\d+\s*=\s*\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(\bAND\b\s+\d+\s*=\s*\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    private static readonly Regex[] XssPatterns =
    [
        new(@"<script[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"on(load|error|click|mouseover)\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"<iframe[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"<img[^>]+onerror\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    private static readonly HashSet<string> SuspiciousMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "TRACE", "TRACK", "CONNECT", "OPTIONS"
    };

    private static readonly Dictionary<string, (int Count, DateTime Window)> _requestTracker = new();
    private const int RateLimitThreshold = 50;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
        var path = context.Request.Path.ToString();
        var query = context.Request.QueryString.ToString();
        var fullUrl = $"{path}{query}";

        // Check for suspicious HTTP methods
        if (SuspiciousMethods.Contains(context.Request.Method))
        {
            _logger.LogWarning(
                "SECURITY: Suspicious HTTP method {Method} from {ClientIp} on {Path} | CorrelationId: {CorrelationId}",
                context.Request.Method, clientIp, path, correlationId);
        }

        // Check for SQL injection patterns
        foreach (var pattern in SqlInjectionPatterns)
        {
            if (pattern.IsMatch(fullUrl))
            {
                _logger.LogCritical(
                    "SECURITY: Possible SQL Injection detected from {ClientIp} | Pattern: {Pattern} | URL: {Url} | CorrelationId: {CorrelationId}",
                    clientIp, pattern.ToString(), fullUrl, correlationId);
                break;
            }
        }

        // Check for XSS patterns
        foreach (var pattern in XssPatterns)
        {
            if (pattern.IsMatch(fullUrl))
            {
                _logger.LogCritical(
                    "SECURITY: Possible XSS attempt detected from {ClientIp} | Pattern: {Pattern} | URL: {Url} | CorrelationId: {CorrelationId}",
                    clientIp, pattern.ToString(), fullUrl, correlationId);
                break;
            }
        }

        // Rate limiting check
        CheckRateLimit(clientIp, correlationId);

        await _next(context);
    }

    private void CheckRateLimit(string clientIp, string correlationId)
    {
        var now = DateTime.UtcNow;

        lock (_requestTracker)
        {
            if (_requestTracker.TryGetValue(clientIp, out var entry))
            {
                if (now - entry.Window < RateLimitWindow)
                {
                    var newCount = entry.Count + 1;
                    _requestTracker[clientIp] = (newCount, entry.Window);

                    if (newCount > RateLimitThreshold)
                    {
                        _logger.LogWarning(
                            "SECURITY: Rate limit exceeded for {ClientIp} | Requests: {Count} in last minute | CorrelationId: {CorrelationId}",
                            clientIp, newCount, correlationId);
                    }
                }
                else
                {
                    _requestTracker[clientIp] = (1, now);
                }
            }
            else
            {
                _requestTracker[clientIp] = (1, now);
            }
        }
    }
}
