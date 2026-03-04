using Microsoft.AspNetCore.Mvc;

namespace DataProtectionLabApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
