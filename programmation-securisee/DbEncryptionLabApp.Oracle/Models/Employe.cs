namespace DbEncryptionLabApp.Oracle.Models;

/// <summary>
/// Base employee entity — used across all encryption patterns.
/// Each pattern has its own table with the same structure.
/// </summary>
public class EmployeClair
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string NumeroNational { get; set; } = string.Empty;   // Sensitive: Belgian SSN
    public string Salaire { get; set; } = string.Empty;          // Sensitive: Salary
    public string CompteBancaire { get; set; } = string.Empty;   // Sensitive: IBAN
    public string Email { get; set; } = string.Empty;            // Non-sensitive
}

public class EmployeValueConverter
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string NumeroNational { get; set; } = string.Empty;
    public string Salaire { get; set; } = string.Empty;
    public string CompteBancaire { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class EmployeAes
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string NumeroNational { get; set; } = string.Empty;
    public string Salaire { get; set; } = string.Empty;
    public string CompteBancaire { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
