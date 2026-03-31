using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KeycloakAuthLabApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AuthApiController : ControllerBase
{
    /// <summary>
    /// Returns the current user's information extracted from the JWT token.
    /// </summary>
    [HttpGet("userinfo")]
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Name);
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Name = name,
            Roles = roles,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    /// <summary>
    /// Token introspection - returns details about the current token.
    /// </summary>
    [HttpGet("token-info")]
    public IActionResult GetTokenInfo()
    {
        var expClaim = User.FindFirstValue("exp");
        var iatClaim = User.FindFirstValue("iat");
        var issClaim = User.FindFirstValue("iss");
        var audClaim = User.FindFirstValue("aud");

        return Ok(new
        {
            Issuer = issClaim,
            Audience = audClaim,
            IssuedAt = iatClaim,
            Expiration = expClaim,
            Subject = User.FindFirstValue(ClaimTypes.NameIdentifier),
            TokenType = "Bearer",
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }

    /// <summary>
    /// Public endpoint for testing - no authentication required.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new
        {
            Message = "Cet endpoint est public et accessible sans authentification.",
            Timestamp = DateTime.UtcNow
        });
    }
}
