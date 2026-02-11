using Microsoft.AspNetCore.Mvc;
using SqlInjectionLabApp.Models;
using SqlInjectionLabApp.Services;

namespace SqlInjectionLabApp.Controllers;

public class SearchController : Controller
{
    private readonly DatabaseService _db;

    public SearchController(DatabaseService db)
    {
        _db = db;
    }

    // ============================================================
    // RECHERCHE VULNERABLE
    // ============================================================
    public IActionResult Vulnerable(string? q)
    {
        var model = new SearchViewModel
        {
            SearchTerm = q,
            IsVulnerable = true,
            SuggestedPayloads =
            [
                "' OR '1'='1",
                "' OR 1=1 --",
                "' UNION SELECT 1,Username,Password,Email,Role,'','','','','','',0,'' FROM Users --",
                "' UNION SELECT 1,Key,Value,'','','','','','','','',0,'' FROM SecretConfig --",
                "'; DROP TABLE Persons; --",
            ]
        };

        if (!string.IsNullOrEmpty(q))
        {
            var (results, query, error) = _db.SearchVulnerable(q);
            model.Results = results;
            model.ExecutedQuery = query;
            model.ErrorMessage = error;
        }

        return View(model);
    }

    // ============================================================
    // RECHERCHE SECURISEE
    // ============================================================
    public IActionResult Secure(string? q)
    {
        var model = new SearchViewModel
        {
            SearchTerm = q,
            IsVulnerable = false,
        };

        if (!string.IsNullOrEmpty(q))
        {
            var (results, query, error) = _db.SearchSecure(q);
            model.Results = results;
            model.ExecutedQuery = query;
            model.ErrorMessage = error;
        }

        return View(model);
    }
}
