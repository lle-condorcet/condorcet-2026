using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace KeycloakAuthLabApp.Authorization;

/// <summary>
/// Transforms Keycloak JWT claims to .NET ClaimsPrincipal roles.
/// Keycloak stores roles in realm_access.roles and resource_access.{client}.roles.
/// This transformer maps them to standard ClaimTypes.Role claims.
/// </summary>
public class KeycloakRoleTransformer : IClaimsTransformation
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakRoleTransformer> _logger;

    public KeycloakRoleTransformer(IConfiguration configuration, ILogger<KeycloakRoleTransformer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        // Avoid adding duplicate roles if already transformed
        var existingRoles = identity.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToHashSet();

        // Extract realm_access.roles from JWT
        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim != null)
        {
            try
            {
                var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
                if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (roleName != null && !existingRoles.Contains(roleName))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            existingRoles.Add(roleName);
                            _logger.LogDebug("Added realm role: {Role}", roleName);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse realm_access claim");
            }
        }

        // Extract resource_access.{clientId}.roles from JWT
        var clientId = _configuration["Keycloak:ClientId"] ?? "securite-lab-app";
        var resourceAccessClaim = principal.FindFirst("resource_access");
        if (resourceAccessClaim != null)
        {
            try
            {
                var resourceAccess = JsonDocument.Parse(resourceAccessClaim.Value);
                if (resourceAccess.RootElement.TryGetProperty(clientId, out var clientAccess) &&
                    clientAccess.TryGetProperty("roles", out var roles))
                {
                    foreach (var role in roles.EnumerateArray())
                    {
                        var roleName = role.GetString();
                        if (roleName != null && !existingRoles.Contains(roleName))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            existingRoles.Add(roleName);
                            _logger.LogDebug("Added client role: {Role}", roleName);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse resource_access claim");
            }
        }

        return Task.FromResult(principal);
    }
}
