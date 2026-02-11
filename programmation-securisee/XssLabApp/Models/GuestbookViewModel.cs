namespace XssLabApp.Models;

public class GuestbookViewModel
{
    public List<Comment> Comments { get; set; } = [];
    public string NewAuthor { get; set; } = "";
    public string NewContent { get; set; } = "";
    public string? XssPayload { get; set; }
}
