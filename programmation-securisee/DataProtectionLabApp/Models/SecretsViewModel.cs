namespace DataProtectionLabApp.Models;

public class SecretsViewModel
{
    public Dictionary<string, string> ExposedSecrets { get; set; } = new();
    public Dictionary<string, string> MaskedSecrets { get; set; } = new();
    public string? AppSettingsContent { get; set; }
    public string? UserSecretsPath { get; set; }
    public List<string> Commands { get; set; } = new();
    public string? ConfigSource { get; set; }
}
