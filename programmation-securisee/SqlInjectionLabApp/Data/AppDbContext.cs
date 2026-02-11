using Microsoft.EntityFrameworkCore;
using SqlInjectionLabApp.Models;

namespace SqlInjectionLabApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<UserAccount> Users => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().ToTable("EfPersons");
        modelBuilder.Entity<UserAccount>().ToTable("EfUsers");

        // Seed : memes donnees que le labo ADO.NET
        modelBuilder.Entity<Person>().HasData(
            new Person { Id = 1, FirstName = "Jean", LastName = "Dupont", Email = "jean.dupont@mail.be", Phone = "+32 471 23 45 67", Address = "Rue de la Loi 42", City = "Bruxelles", NationalId = "85.07.15-123.45", DateOfBirth = "1985-07-15", CreditCard = "4532-XXXX-XXXX-8901", Iban = "BE68 5390 0754 7034", Salary = 3250.00m, Department = "Comptabilite" },
            new Person { Id = 2, FirstName = "Marie", LastName = "Lecomte", Email = "marie.lecomte@mail.be", Phone = "+32 485 67 89 01", Address = "Avenue Louise 156", City = "Bruxelles", NationalId = "90.03.22-456.78", DateOfBirth = "1990-03-22", CreditCard = "5412-XXXX-XXXX-3456", Iban = "BE71 0961 2345 6789", Salary = 4100.00m, Department = "Direction" },
            new Person { Id = 3, FirstName = "Pierre", LastName = "Vandenbroeck", Email = "pierre.vdb@mail.be", Phone = "+32 476 11 22 33", Address = "Grote Markt 8", City = "Antwerpen", NationalId = "78.11.30-789.01", DateOfBirth = "1978-11-30", CreditCard = "4916-XXXX-XXXX-7890", Iban = "BE43 0689 9999 0023", Salary = 2800.00m, Department = "IT" },
            new Person { Id = 4, FirstName = "Sophie", LastName = "Martin", Email = "sophie.martin@mail.be", Phone = "+32 492 44 55 66", Address = "Rue Hors-Chateau 12", City = "Liege", NationalId = "95.06.08-234.56", DateOfBirth = "1995-06-08", CreditCard = "4539-XXXX-XXXX-1234", Iban = "BE09 7340 5577 1136", Salary = 3500.00m, Department = "RH" },
            new Person { Id = 5, FirstName = "Luc", LastName = "Dubois", Email = "luc.dubois@mail.be", Phone = "+32 478 77 88 99", Address = "Veldstraat 45", City = "Gent", NationalId = "82.12.01-567.89", DateOfBirth = "1982-12-01", CreditCard = "5425-XXXX-XXXX-5678", Iban = "BE62 5100 0754 7061", Salary = 3100.00m, Department = "Comptabilite" },
            new Person { Id = 6, FirstName = "Isabelle", LastName = "Claessens", Email = "isabelle.c@mail.be", Phone = "+32 486 00 11 22", Address = "Bd de la Sauveniere 7", City = "Liege", NationalId = "88.09.14-890.12", DateOfBirth = "1988-09-14", CreditCard = "4556-XXXX-XXXX-9012", Iban = "BE15 2100 0654 3178", Salary = 4500.00m, Department = "Direction" },
            new Person { Id = 7, FirstName = "Thomas", LastName = "De Smedt", Email = "thomas.ds@mail.be", Phone = "+32 473 33 44 55", Address = "Meir 100", City = "Antwerpen", NationalId = "92.01.25-345.67", DateOfBirth = "1992-01-25", CreditCard = "4916-XXXX-XXXX-3456", Iban = "BE38 0017 5469 8213", Salary = 2950.00m, Department = "IT" },
            new Person { Id = 8, FirstName = "Amelie", LastName = "Fontaine", Email = "amelie.f@mail.be", Phone = "+32 495 66 77 88", Address = "Rue du Midi 23", City = "Charleroi", NationalId = "97.04.17-678.90", DateOfBirth = "1997-04-17", CreditCard = "5412-XXXX-XXXX-7890", Iban = "BE55 7330 2145 3697", Salary = 2700.00m, Department = "Support" },
            new Person { Id = 9, FirstName = "Nicolas", LastName = "Janssens", Email = "nicolas.j@mail.be", Phone = "+32 477 99 00 11", Address = "Grand Place 1", City = "Mons", NationalId = "80.08.03-901.23", DateOfBirth = "1980-08-03", CreditCard = "4532-XXXX-XXXX-1234", Iban = "BE82 6528 3177 4092", Salary = 3800.00m, Department = "IT" },
            new Person { Id = 10, FirstName = "Caroline", LastName = "Hermans", Email = "caroline.h@mail.be", Phone = "+32 489 22 33 44", Address = "Bondgenotenlaan 50", City = "Leuven", NationalId = "93.02.28-012.34", DateOfBirth = "1993-02-28", CreditCard = "4539-XXXX-XXXX-5678", Iban = "BE91 7512 0489 5631", Salary = 3400.00m, Department = "RH" }
        );

        modelBuilder.Entity<UserAccount>().HasData(
            new UserAccount { Id = 1, Username = "admin", Password = "Admin@2026!", Role = "admin", Email = "admin@condorcet.be" },
            new UserAccount { Id = 2, Username = "jean.dupont", Password = "P@ssword1", Role = "user", Email = "jean.dupont@mail.be" },
            new UserAccount { Id = 3, Username = "marie.lecomte", Password = "Marie1990!", Role = "manager", Email = "marie.lecomte@mail.be" },
            new UserAccount { Id = 4, Username = "dba_backup", Password = "Sup3rS3cret!", Role = "admin", Email = "dba@condorcet.be" }
        );
    }
}
