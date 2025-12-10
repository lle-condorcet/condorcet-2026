namespace WebApplicationRazorPages.Persistence.Entities;

public class Person
{
    // public Guid Id { get; set; }
    // Naming convention : Id => always Primary Key as implicit conversion
    // public int PersonId { get; set; }
    public int Id { get; set; }
    // [Key]
    // public int MyPrimaryKey { get; set; }
    public string FullName { get; set; }
    public string? Email { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IsAlive { get; set; }
    
    // Creates a foreign key and a relationship between
    // Job & Person tables
    public int JobId { get; set; }
    public Job Job { get; set; }
}