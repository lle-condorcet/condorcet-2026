using Microsoft.AspNetCore.Mvc;
using XssLabApp.Models;
using XssLabApp.Services;

namespace XssLabApp.Controllers;

/// <summary>
/// Simule le serveur de l'attaquant qui recoit les donnees exfiltrees.
/// Dans un vrai scenario, ce endpoint serait sur un serveur externe controle par l'attaquant.
/// Ici, il est dans la meme app pour les besoins du labo.
/// </summary>
[ApiController]
[Route("api/exfiltration")]
public class ExfiltrationApiController : ControllerBase
{
    private readonly CaptureStore _store;

    public ExfiltrationApiController(CaptureStore store)
    {
        _store = store;
    }

    // Endpoint qui recoit le screenshot + donnees volees
    [HttpPost("capture")]
    public IActionResult Capture([FromBody] CaptureRequest request)
    {
        if (string.IsNullOrEmpty(request.Image))
            return BadRequest("No image data");

        _store.Add(new CapturedData
        {
            ScreenshotBase64 = request.Image,
            PageUrl = request.Url ?? "",
            Cookies = request.Cookies ?? "",
            UserAgent = request.UserAgent ?? "",
            CapturedAt = DateTime.Now
        });

        return Ok(new { status = "captured", message = "Screenshot exfiltre avec succes" });
    }
}

public class CaptureRequest
{
    public string Image { get; set; } = "";
    public string? Url { get; set; }
    public string? Cookies { get; set; }
    public string? UserAgent { get; set; }
}
