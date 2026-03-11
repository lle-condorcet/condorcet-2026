using Microsoft.AspNetCore.Mvc;

namespace DbEncryptionLabApp.Controllers;

public class AlwaysEncryptedController : Controller
{
    public IActionResult Index() => View();
}
