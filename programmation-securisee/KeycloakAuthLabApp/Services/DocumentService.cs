using KeycloakAuthLabApp.Data;
using KeycloakAuthLabApp.Models;
using Microsoft.EntityFrameworkCore;

namespace KeycloakAuthLabApp.Services;

public class DocumentService
{
    private readonly AppDbContext _context;
    private readonly AccessLogService _accessLog;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(AppDbContext context, AccessLogService accessLog, ILogger<DocumentService> logger)
    {
        _context = context;
        _accessLog = accessLog;
        _logger = logger;
    }

    /// <summary>
    /// Returns the maximum classification level accessible for a given role.
    /// admin -> Secret, manager -> Confidential, auditor -> Confidential, user -> Internal
    /// </summary>
    public static Classification GetMaxClassificationForRole(string role)
    {
        return role.ToLower() switch
        {
            "admin" => Classification.Secret,
            "manager" => Classification.Confidential,
            "auditor" => Classification.Confidential,
            "user" => Classification.Internal,
            _ => Classification.Public
        };
    }

    public static Classification GetMaxClassificationForRoles(IEnumerable<string> roles)
    {
        var max = Classification.Public;
        foreach (var role in roles)
        {
            var level = GetMaxClassificationForRole(role);
            if (level > max) max = level;
        }
        return max;
    }

    public async Task<List<SecureDocument>> GetAccessibleDocumentsAsync(string userId, IEnumerable<string> roles, string ipAddress)
    {
        var maxLevel = GetMaxClassificationForRoles(roles);

        var documents = await _context.Documents
            .Where(d => d.Classification <= maxLevel)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();

        await _accessLog.LogAsync(userId, "LIST_DOCUMENTS", "Documents", ipAddress, true,
            $"Listed {documents.Count} documents (max level: {maxLevel})");

        return documents;
    }

    public async Task<SecureDocument?> GetDocumentAsync(int id, string userId, IEnumerable<string> roles, string ipAddress)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return null;

        var maxLevel = GetMaxClassificationForRoles(roles);
        if (document.Classification > maxLevel)
        {
            await _accessLog.LogAsync(userId, "VIEW_DOCUMENT", $"Document/{id}", ipAddress, false,
                $"Access denied: document classification {document.Classification} exceeds role level {maxLevel}");
            _logger.LogWarning("User {UserId} denied access to document {DocId} (classification: {Classification})",
                userId, id, document.Classification);
            return null;
        }

        await _accessLog.LogAsync(userId, "VIEW_DOCUMENT", $"Document/{id}", ipAddress, true,
            $"Viewed document: {document.Title}");

        return document;
    }

    public async Task<SecureDocument> CreateDocumentAsync(SecureDocument document, string userId, string ipAddress)
    {
        document.OwnerId = userId;
        document.CreatedAt = DateTime.UtcNow;
        document.UpdatedAt = DateTime.UtcNow;

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        await _accessLog.LogAsync(userId, "CREATE_DOCUMENT", $"Document/{document.Id}", ipAddress, true,
            $"Created document: {document.Title} (classification: {document.Classification})");

        return document;
    }

    public async Task<bool> UpdateDocumentAsync(SecureDocument document, string userId, string ipAddress)
    {
        var existing = await _context.Documents.FindAsync(document.Id);
        if (existing == null) return false;

        existing.Title = document.Title;
        existing.Content = document.Content;
        existing.Classification = document.Classification;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _accessLog.LogAsync(userId, "UPDATE_DOCUMENT", $"Document/{document.Id}", ipAddress, true,
            $"Updated document: {document.Title}");

        return true;
    }

    public async Task<bool> DeleteDocumentAsync(int id, string userId, string ipAddress)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null) return false;

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        await _accessLog.LogAsync(userId, "DELETE_DOCUMENT", $"Document/{id}", ipAddress, true,
            $"Deleted document: {document.Title}");

        return true;
    }
}
