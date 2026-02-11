namespace SqlInjectionLabApp.Models;

public class UserAccount
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";   // En clair (volontairement vulnerable)
    public string Role { get; set; } = "";
    public string Email { get; set; } = "";
}
