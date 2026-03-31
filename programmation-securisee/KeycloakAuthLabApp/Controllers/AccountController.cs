using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeycloakAuthLabApp.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;

    public AccountController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Login(string? returnUrl = "/")
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public IActionResult Profile()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        ViewBag.Claims = claims;
        ViewBag.Roles = roles;
        ViewBag.DisplayName = User.FindFirstValue("preferred_username")
                           ?? User.FindFirstValue(ClaimTypes.Name)
                           ?? "Utilisateur";
        ViewBag.Email = User.FindFirstValue(ClaimTypes.Email) ?? "N/A";
        ViewBag.KeycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "N/A";

        // Try to get access token for display
        ViewBag.AccessToken = HttpContext.GetTokenAsync("access_token").Result;
        ViewBag.IdToken = HttpContext.GetTokenAsync("id_token").Result;
        ViewBag.RefreshToken = HttpContext.GetTokenAsync("refresh_token").Result;

        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
