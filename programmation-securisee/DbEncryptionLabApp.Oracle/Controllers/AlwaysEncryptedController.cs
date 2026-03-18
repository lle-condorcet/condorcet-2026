using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Oracle.Controllers;

public class AlwaysEncryptedController : Controller
{
    public IActionResult Index() => View();
}
