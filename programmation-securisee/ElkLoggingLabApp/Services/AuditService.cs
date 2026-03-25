using Microsoft.EntityFrameworkCore;
using ElkLoggingLabApp.Data;
using ElkLoggingLabApp.Models;

namespace ElkLoggingLabApp.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogActionAsync(string action, string entityType, string entityId, string userId, string details, string ipAddress)
    {
        var auditLog = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Details = details,
            IpAddress = ipAddress
        };

        _db.AuditLogs.Add(auditLog);
        await _db.SaveChangesAsync();

        _logger.LogInformation("AUDIT: {Action} on {EntityType}/{EntityId} by {UserId} from {IpAddress} | {Details}",
            action, entityType, entityId, userId, ipAddress, details);
    }

    public async Task<List<AuditLog>> GetRecentAsync(int count = 50)
    {
        return await _db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> SearchAsync(string? action, string? entityType, DateTime? from, DateTime? to)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).Take(100).ToListAsync();
    }
}
