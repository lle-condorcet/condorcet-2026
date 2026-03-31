namespace KeycloakAuthLabApp.Models;

public enum Classification
{
    Public,
    Internal,
    Confidential,
    Secret
}

public class SecureDocument
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Classification Classification { get; set; } = Classification.Public;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
