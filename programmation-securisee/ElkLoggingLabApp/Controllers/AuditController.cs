using Microsoft.AspNetCore.Mvc;
using ElkLoggingLabApp.Models;
using ElkLoggingLabApp.Services;

namespace ElkLoggingLabApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(AuditService auditService, ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLog>>> GetRecent([FromQuery] int count = 50)
    {
        _logger.LogInformation("Audit logs requested: last {Count} entries", count);
        var logs = await _auditService.GetRecentAsync(count);
        return Ok(logs);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<AuditLog>>> Search(
        [FromQuery] string? action,
        [FromQuery] string? entityType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        _logger.LogInformation("Audit log search: Action={Action}, EntityType={EntityType}, From={From}, To={To}",
            action, entityType, from, to);
        var logs = await _auditService.SearchAsync(action, entityType, from, to);
        return Ok(logs);
    }
}
