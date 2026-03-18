using Microsoft.AspNetCore.Mvc;

namespace CorsLabClient.Controllers;

public class ScenarioController : Controller
{
    public IActionResult NoCors()
    {
        return View();
    }

    public IActionResult Wildcard()
    {
        return View();
    }

    public IActionResult Vulnerable()
    {
        return View();
    }

    public IActionResult Secure()
    {
        return View();
    }
}
