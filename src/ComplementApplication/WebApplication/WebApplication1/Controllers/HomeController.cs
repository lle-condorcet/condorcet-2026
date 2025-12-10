using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogTrace("We are in Index");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogTrace("We are in Privacy");
        return View();
    }

    public IActionResult MyCustomPage()
    {
        _logger.LogTrace("We are in MyCustomPage");
        return View("MyCustomPage");
    }

    // .NET Attribute
    // .NET MVC C# Razor
    // RazorPages >< MVC
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("We are in Error");
        var model = new ErrorViewModel()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        
        // TODO: business logic
        // CQRS - UI/Business Layout
        
        return View(model);
    }

    public void Example()
    {
        var errorViewModel = new ErrorViewModel();
        // RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        if (Activity.Current != null)
        {
            if (Activity.Current.Id != null)
            {
                errorViewModel.RequestId = Activity.Current.Id;
            }
            else
            {
                errorViewModel.RequestId = HttpContext.TraceIdentifier;
            }
        }
        else
        {
            errorViewModel.RequestId = HttpContext.TraceIdentifier;
        }
    }
}