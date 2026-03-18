namespace DbEncryptionLabApp.Postgres.Models;

public class ComparisonViewModel
{
    public List<EmployeClair> EmployesClair { get; set; } = [];
    public List<EmployeValueConverter> EmployesValueConverter { get; set; } = [];
    public List<EmployeAes> EmployesAes { get; set; } = [];

    // Raw rows as stored in PostgreSQL (encrypted values)
    public List<Dictionary<string, string>> RawRowsClair { get; set; } = [];
    public List<Dictionary<string, string>> RawRowsValueConverter { get; set; } = [];
    public List<Dictionary<string, string>> RawRowsAes { get; set; } = [];
}

public class PatternViewModel
{
    public string PatternName { get; set; } = string.Empty;
    public string PatternDescription { get; set; } = string.Empty;

    // Application view (decrypted)
    public List<Dictionary<string, string>> AppRows { get; set; } = [];

    // Database view (raw)
    public List<Dictionary<string, string>> DbRows { get; set; } = [];
}
