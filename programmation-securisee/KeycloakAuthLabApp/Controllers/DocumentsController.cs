using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KeycloakAuthLabApp.Models;
using KeycloakAuthLabApp.Services;
using System.Security.Claims;

namespace KeycloakAuthLabApp.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly DocumentService _documentService;

    public DocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
    private IEnumerable<string> GetRoles() => User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
    private string GetIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    public async Task<IActionResult> Index()
    {
        var documents = await _documentService.GetAccessibleDocumentsAsync(GetUserId(), GetRoles(), GetIpAddress());
        var maxLevel = DocumentService.GetMaxClassificationForRoles(GetRoles());
        ViewBag.MaxClassification = maxLevel;
        return View(documents);
    }

    public async Task<IActionResult> Details(int id)
    {
        var document = await _documentService.GetDocumentAsync(id, GetUserId(), GetRoles(), GetIpAddress());
        if (document == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        return View(document);
    }

    [Authorize(Policy = "RequireManager")]
    public IActionResult Create()
    {
        return View(new SecureDocument());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "RequireManager")]
    public async Task<IActionResult> Create(SecureDocument document)
    {
        if (!ModelState.IsValid)
        {
            return View(document);
        }

        await _documentService.CreateDocumentAsync(document, GetUserId(), GetIpAddress());
        return RedirectToAction(nameof(Index));
    }
}
