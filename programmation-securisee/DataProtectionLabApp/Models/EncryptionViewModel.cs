namespace DataProtectionLabApp.Models;

public class EncryptionViewModel
{
    public string? PlainText { get; set; }
    public string? CipherText { get; set; }
    public List<SensitiveRecord> Records { get; set; } = new();
    public bool IsEncrypted { get; set; }
    public string? TimeLimitedCipher { get; set; }
    public DateTime? Expiration { get; set; }
}
