namespace XssLabApp.Models;

public class CapturedData
{
    public int Id { get; set; }
    public string ScreenshotBase64 { get; set; } = "";
    public string PageUrl { get; set; } = "";
    public string Cookies { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public DateTime CapturedAt { get; set; } = DateTime.Now;
}
