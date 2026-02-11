using Microsoft.AspNetCore.Mvc;
using XssLabApp.Services;

namespace XssLabApp.Controllers;

/// <summary>
/// Dashboard de l'attaquant - visualise les captures exfiltrees.
/// </summary>
public class AttackerController : Controller
{
    private readonly CaptureStore _store;

    public AttackerController(CaptureStore store)
    {
        _store = store;
    }

    public IActionResult Dashboard()
    {
        var captures = _store.GetAll();
        return View(captures);
    }

    [HttpPost]
    public IActionResult Clear()
    {
        _store.Clear();
        return RedirectToAction("Dashboard");
    }
}
