using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DataProtectionLabApp.Models;

namespace DataProtectionLabApp.Data;

public class EncryptedDbContext : DbContext
{
    private readonly IDataProtector _protector;

    public DbSet<Patient> Patients => Set<Patient>();

    public EncryptedDbContext(DbContextOptions<EncryptedDbContext> options, IDataProtectionProvider provider)
        : base(options)
    {
        _protector = provider.CreateProtector("DataProtectionLab.EFCore.Encryption");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Value converter that encrypts on save and decrypts on read
        var encryptedConverter = new ValueConverter<string, string>(
            v => _protector.Protect(v),       // Write to DB → encrypt
            v => _protector.Unprotect(v)      // Read from DB → decrypt
        );

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.Property(p => p.NumeroNational).HasConversion(encryptedConverter);
            entity.Property(p => p.Diagnostic).HasConversion(encryptedConverter);
            entity.Property(p => p.NoteMedicale).HasConversion(encryptedConverter);

            // Nom and Prenom are stored in clear (non-sensitive)
        });
    }
}
