namespace SqlInjectionLabApp.Models;

public class SearchViewModel
{
    public string? SearchTerm { get; set; }
    public List<Person> Results { get; set; } = [];
    public string? ExecutedQuery { get; set; }     // La requete SQL executee (pour montrer aux etudiants)
    public string? ErrorMessage { get; set; }
    public bool IsVulnerable { get; set; }
    public List<string> SuggestedPayloads { get; set; } = [];
}
