using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuthLabApp.Services;

namespace KeycloakAuthLabApp.Controllers;

[Authorize(Policy = "RequireAdmin")]
public class AdminController : Controller
{
    private readonly KeycloakAdminService _keycloakAdmin;
    private readonly AccessLogService _accessLog;

    public AdminController(KeycloakAdminService keycloakAdmin, AccessLogService accessLog)
    {
        _keycloakAdmin = keycloakAdmin;
        _accessLog = accessLog;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _keycloakAdmin.GetUsersAsync();
        var roles = await _keycloakAdmin.GetRealmRolesAsync();
        var recentLogs = await _accessLog.GetRecentLogsAsync(50);
        var failedLogs = await _accessLog.GetFailedAccessLogsAsync(20);

        ViewBag.Users = users;
        ViewBag.Roles = roles;
        ViewBag.RecentLogs = recentLogs;
        ViewBag.FailedLogs = failedLogs;

        return View();
    }
}
