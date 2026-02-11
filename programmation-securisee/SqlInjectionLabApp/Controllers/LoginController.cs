using Microsoft.AspNetCore.Mvc;
using SqlInjectionLabApp.Models;
using SqlInjectionLabApp.Services;

namespace SqlInjectionLabApp.Controllers;

public class LoginController : Controller
{
    private readonly DatabaseService _db;

    public LoginController(DatabaseService db)
    {
        _db = db;
    }

    // ============================================================
    // LOGIN VULNERABLE
    // ============================================================
    public IActionResult Vulnerable()
    {
        return View(new LoginViewModel
        {
            IsVulnerable = true,
            SuggestedPayloads =
            [
                "' OR '1'='1' --",
                "admin' --",
                "' OR 1=1 --",
                "' UNION SELECT 1,'hacked','','admin','hacked@evil.com' --",
            ]
        });
    }

    [HttpPost]
    public IActionResult Vulnerable(string username, string password)
    {
        var (user, query, error) = _db.LoginVulnerable(username ?? "", password ?? "");

        var model = new LoginViewModel
        {
            Username = username,
            Password = password,
            ExecutedQuery = query,
            ErrorMessage = error,
            LoggedInUser = user,
            IsVulnerable = true,
            SuggestedPayloads =
            [
                "' OR '1'='1' --",
                "admin' --",
                "' OR 1=1 --",
                "' UNION SELECT 1,'hacked','','admin','hacked@evil.com' --",
            ]
        };

        if (user != null)
            model.SuccessMessage = $"Connecte en tant que : {user.Username} (role: {user.Role})";
        else if (error == null)
            model.ErrorMessage = "Identifiants invalides.";

        return View(model);
    }

    // ============================================================
    // LOGIN SECURISE
    // ============================================================
    public IActionResult Secure()
    {
        return View(new LoginViewModel { IsVulnerable = false });
    }

    [HttpPost]
    public IActionResult Secure(string username, string password)
    {
        var (user, query, error) = _db.LoginSecure(username ?? "", password ?? "");

        var model = new LoginViewModel
        {
            Username = username,
            Password = password,
            ExecutedQuery = query,
            ErrorMessage = error,
            IsVulnerable = false,
        };

        if (user != null)
            model.SuccessMessage = $"Connecte en tant que : {user.Username} (role: {user.Role})";
        else if (error == null)
            model.ErrorMessage = "Identifiants invalides.";

        return View(model);
    }
}
