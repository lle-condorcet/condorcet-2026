namespace SqlInjectionLabApp.Models;

public class EfDemoViewModel
{
    public string? SearchTerm { get; set; }
    public List<Person> Results { get; set; } = [];
    public string? GeneratedSql { get; set; }
    public string? ErrorMessage { get; set; }
    public string Approach { get; set; } = "";          // "FromSqlRaw", "LINQ", "FromSqlInterpolated"
    public bool IsVulnerable { get; set; }
}
