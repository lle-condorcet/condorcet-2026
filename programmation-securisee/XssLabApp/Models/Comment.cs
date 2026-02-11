namespace XssLabApp.Models;

public class Comment
{
    public int Id { get; set; }
    public string Author { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime PostedAt { get; set; } = DateTime.Now;
}
