using Microsoft.AspNetCore.Authorization;
using KeycloakAuthLabApp.Models;

namespace KeycloakAuthLabApp.Authorization;

/// <summary>
/// Authorization requirement that checks if a user's role grants access
/// to a specific document classification level.
/// </summary>
public class ClassificationRequirement : IAuthorizationRequirement
{
    public Classification RequiredClassification { get; }

    public ClassificationRequirement(Classification classification)
    {
        RequiredClassification = classification;
    }
}

public class ClassificationHandler : AuthorizationHandler<ClassificationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClassificationRequirement requirement)
    {
        var roles = context.User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!roles.Any())
        {
            return Task.CompletedTask;
        }

        var maxLevel = Services.DocumentService.GetMaxClassificationForRoles(roles);

        if (maxLevel >= requirement.RequiredClassification)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
