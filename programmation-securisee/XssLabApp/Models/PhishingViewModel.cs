namespace XssLabApp.Models;

public class PhishingViewModel
{
    public string Name { get; set; } = "";
    public string? XssPayload { get; set; }
}
