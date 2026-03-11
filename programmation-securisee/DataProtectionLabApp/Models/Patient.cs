namespace DataProtectionLabApp.Models;

public class Patient
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;

    // These fields will be encrypted at rest via EF Core Value Converters
    public string NumeroNational { get; set; } = string.Empty;
    public string Diagnostic { get; set; } = string.Empty;
    public string NoteMedicale { get; set; } = string.Empty;
}
