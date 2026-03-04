namespace DataProtectionLabApp.Models;

public class SensitiveRecord
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? EncryptedValue { get; set; }
}
