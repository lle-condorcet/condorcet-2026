namespace SqlInjectionLabApp.Models;

public class LoginViewModel
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ExecutedQuery { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public UserAccount? LoggedInUser { get; set; }
    public bool IsVulnerable { get; set; }
    public List<string> SuggestedPayloads { get; set; } = [];
}
