using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Postgres.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
