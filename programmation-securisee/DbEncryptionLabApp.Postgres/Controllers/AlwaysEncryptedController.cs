using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Postgres.Controllers;

public class AlwaysEncryptedController : Controller
{
    public IActionResult Index() => View();
}
