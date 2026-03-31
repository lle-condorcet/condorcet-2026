using KeycloakAuthLabApp.Data;
using KeycloakAuthLabApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuthLabApp.Services;

public class AccessLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AccessLogService> _logger;

    public AccessLogService(AppDbContext context, ILogger<AccessLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string userId, string action, string resource, string ipAddress, bool success, string details = "")
    {
        var log = new AccessLog
        {
            UserId = userId,
            Action = action,
            Resource = resource,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            Success = success,
            Details = details
        };

        _context.AccessLogs.Add(log);
        await _context.SaveChangesAsync();

        if (!success)
        {
            _logger.LogWarning("Access denied: User={UserId}, Action={Action}, Resource={Resource}, IP={IP}",
                userId, action, resource, ipAddress);
        }
    }

    public async Task<List<AccessLog>> GetRecentLogsAsync(int count = 50)
    {
        return await _context.AccessLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AccessLog>> GetUserLogsAsync(string userId, int count = 20)
    {
        return await _context.AccessLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AccessLog>> GetFailedAccessLogsAsync(int count = 50)
    {
        return await _context.AccessLogs
            .Where(l => !l.Success)
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToListAsync();
    }
}
