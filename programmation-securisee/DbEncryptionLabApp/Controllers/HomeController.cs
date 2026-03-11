using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
