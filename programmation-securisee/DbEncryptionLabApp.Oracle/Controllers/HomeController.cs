using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Oracle.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
