using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuthLabApp.Models;
using KeycloakAuthLabApp.Services;
using System.Security.Claims;

namespace KeycloakAuthLabApp.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DocumentsApiController : ControllerBase
{
    private readonly DocumentService _documentService;

    public DocumentsApiController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
    private IEnumerable<string> GetRoles() => User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    /// <summary>
    /// Get all documents accessible to the authenticated user based on their roles.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await _documentService.GetAccessibleDocumentsAsync(GetUserId(), GetRoles(), GetIpAddress());
        return Ok(documents.Select(d => new
        {
            d.Id,
            d.Title,
            Classification = d.Classification.ToString(),
            d.OwnerId,
            d.CreatedAt,
            d.UpdatedAt
        }));
    }

    /// <summary>
    /// Get a specific document by ID (if the user has sufficient access level).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _documentService.GetDocumentAsync(id, GetUserId(), GetRoles(), GetIpAddress());
        if (document == null)
            return Forbid();

        return Ok(new
        {
            document.Id,
            document.Title,
            document.Content,
            Classification = document.Classification.ToString(),
            document.OwnerId,
            document.CreatedAt,
            document.UpdatedAt
        });
    }

    /// <summary>
    /// Create a new document (requires Manager or Admin role).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        if (!Enum.TryParse<Classification>(request.Classification, out var classification))
            return BadRequest(new { Error = "Classification invalide. Valeurs acceptees: Public, Internal, Confidential, Secret" });

        var document = new SecureDocument
        {
            Title = request.Title,
            Content = request.Content,
            Classification = classification
        };

        var created = await _documentService.CreateDocumentAsync(document, GetUserId(), GetIpAddress());
        return CreatedAtAction(nameof(GetDocument), new { id = created.Id }, new
        {
            created.Id,
            created.Title,
            Classification = created.Classification.ToString(),
            created.CreatedAt
        });
    }
}

public record CreateDocumentRequest(string Title, string Content, string Classification);
