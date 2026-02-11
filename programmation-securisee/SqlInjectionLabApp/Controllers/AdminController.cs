using Microsoft.AspNetCore.Mvc;
using SqlInjectionLabApp.Services;

namespace SqlInjectionLabApp.Controllers;

/// <summary>
/// Console SQL brute et reset de la BDD (pour exploration avancee).
/// </summary>
public class AdminController : Controller
{
    private readonly DatabaseService _db;

    public AdminController(DatabaseService db)
    {
        _db = db;
    }

    public IActionResult Console()
    {
        ViewBag.Results = new List<Dictionary<string, object?>>();
        ViewBag.Error = (string?)null;
        ViewBag.Query = (string?)null;
        return View();
    }

    [HttpPost]
    public IActionResult Console(string sql)
    {
        var (results, error) = _db.ExecuteRawQuery(sql ?? "");
        ViewBag.Results = results;
        ViewBag.Error = error;
        ViewBag.Query = sql;
        return View();
    }

    [HttpPost]
    public IActionResult ResetDatabase()
    {
        _db.ResetDatabase();
        TempData["Message"] = "Base de donnees reinitialisee avec succes.";
        return RedirectToAction("Console");
    }
}
