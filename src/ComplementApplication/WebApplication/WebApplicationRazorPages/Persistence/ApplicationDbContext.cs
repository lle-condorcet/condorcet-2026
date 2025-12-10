using Microsoft.EntityFrameworkCore;
using WebApplicationRazorPages.Persistence.Entities;

namespace WebApplicationRazorPages.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Person> Person { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 1, Name = "Employé" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 2, Name = "Ouvrier" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 3, Name = "Indépendant" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 4, Name = "Chef d'entreprise" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 5, Name = "Cadre" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 6, Name = "Profession libérale" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 7, Name = "Fonctionnaire" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 8, Name = "Enseignant" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 9, Name = "Politicien" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 10, Name = "Journaliste" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 11, Name = "Artiste" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 12, Name = "Artisan" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 13, Name = "Retraité" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 14, Name = "Etudiant" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 15, Name = "Demandeur d'emploi" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 16, Name = "Sans" });
        modelBuilder.Entity<Job>().HasData(new Job() { Id = 17, Name = "Autre" });

        modelBuilder.Entity<Person>().HasData(new Person()
        {
            Id = 1,
            FullName = "John Doe",
            Email = "john.doe@test.be",
            BirthDate = new DateTime(1970, 7, 23),
            IsAlive = true,
            JobId = 6
        });
    }
}