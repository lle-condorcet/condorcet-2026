namespace SqlInjectionLabApp.Models;

public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string NationalId { get; set; } = "";        // Numero de registre national
    public string DateOfBirth { get; set; } = "";
    public string CreditCard { get; set; } = "";         // Numero CC (simule)
    public string Iban { get; set; } = "";
    public decimal Salary { get; set; }
    public string Department { get; set; } = "";
}
