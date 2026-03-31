using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuthLabApp.Services;
using System.Security.Claims;

namespace KeycloakAuthLabApp.Controllers;

public class HomeController : Controller
{
    private readonly AccessLogService _accessLog;

    public HomeController(AccessLogService accessLog)
    {
        _accessLog = accessLog;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        var recentLogs = await _accessLog.GetUserLogsAsync(userId, 10);

        ViewBag.Roles = roles;
        ViewBag.RecentLogs = recentLogs;
        ViewBag.DisplayName = User.FindFirstValue("preferred_username")
                           ?? User.FindFirstValue(ClaimTypes.Name)
                           ?? "Utilisateur";

        return View();
    }
}
