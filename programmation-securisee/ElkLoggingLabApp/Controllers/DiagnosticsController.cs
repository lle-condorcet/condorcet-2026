using Microsoft.AspNetCore.Mvc;

namespace ElkLoggingLabApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simulates a slow database query for ELK monitoring demo.
    /// </summary>
    [HttpGet("slow-query")]
    public async Task<IActionResult> SlowQuery()
    {
        _logger.LogWarning("DIAGNOSTICS: Simulating slow database query...");
        var start = DateTime.UtcNow;

        await Task.Delay(Random.Shared.Next(800, 2000));

        var duration = (DateTime.UtcNow - start).TotalMilliseconds;
        _logger.LogError("SLOW QUERY DETECTED: Simulated query took {Duration}ms (threshold: 500ms)", duration);

        return Ok(new
        {
            type = "slow-query",
            duration = $"{duration:F0}ms",
            message = "Simulated slow query completed",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Deliberately triggers an unhandled exception for error logging demo.
    /// </summary>
    [HttpGet("exception")]
    public IActionResult TriggerException()
    {
        _logger.LogWarning("DIAGNOSTICS: About to throw a deliberate exception...");

        var exceptionType = Random.Shared.Next(3);
        throw exceptionType switch
        {
            0 => new InvalidOperationException("Simulated database connection failure"),
            1 => new TimeoutException("Simulated external service timeout after 30000ms"),
            _ => new ApplicationException("Simulated critical application error in order processing pipeline")
        };
    }

    /// <summary>
    /// Simulates high memory allocation for resource monitoring demo.
    /// </summary>
    [HttpGet("memory-pressure")]
    public IActionResult MemoryPressure()
    {
        _logger.LogWarning("DIAGNOSTICS: Simulating memory pressure...");

        var allocations = new List<byte[]>();
        var totalBytes = 0L;

        for (int i = 0; i < 10; i++)
        {
            var size = 1024 * 1024; // 1 MB per allocation
            allocations.Add(new byte[size]);
            totalBytes += size;
        }

        _logger.LogWarning("DIAGNOSTICS: Allocated {TotalMB}MB of memory for demo. GC will reclaim after request.",
            totalBytes / (1024 * 1024));

        var memInfo = GC.GetGCMemoryInfo();

        return Ok(new
        {
            type = "memory-pressure",
            allocatedMB = totalBytes / (1024 * 1024),
            totalMemoryMB = GC.GetTotalMemory(false) / (1024 * 1024),
            gen0Collections = GC.CollectionCount(0),
            gen1Collections = GC.CollectionCount(1),
            gen2Collections = GC.CollectionCount(2),
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Generates a burst of log messages at various levels for testing ELK ingestion.
    /// </summary>
    [HttpGet("log-burst")]
    public IActionResult LogBurst([FromQuery] int count = 20)
    {
        count = Math.Min(count, 100);
        _logger.LogInformation("DIAGNOSTICS: Generating burst of {Count} log messages", count);

        for (int i = 0; i < count; i++)
        {
            var level = i % 5;
            switch (level)
            {
                case 0:
                    _logger.LogTrace("BURST [{Index}/{Total}]: Trace-level diagnostic message", i + 1, count);
                    break;
                case 1:
                    _logger.LogDebug("BURST [{Index}/{Total}]: Debug details for troubleshooting", i + 1, count);
                    break;
                case 2:
                    _logger.LogInformation("BURST [{Index}/{Total}]: Standard informational message", i + 1, count);
                    break;
                case 3:
                    _logger.LogWarning("BURST [{Index}/{Total}]: Warning - potential issue detected", i + 1, count);
                    break;
                case 4:
                    _logger.LogError("BURST [{Index}/{Total}]: Error - simulated failure in component", i + 1, count);
                    break;
            }
        }

        return Ok(new
        {
            type = "log-burst",
            messagesGenerated = count,
            message = $"Generated {count} log messages at various levels",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Health check endpoint with detailed diagnostics.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            uptime = Environment.TickCount64 / 1000,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
            machineName = Environment.MachineName,
            processId = Environment.ProcessId,
            timestamp = DateTime.UtcNow
        });
    }
}
