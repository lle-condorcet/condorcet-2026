using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DbEncryptionLabApp.Models;
using DbEncryptionLabApp.Services;

namespace DbEncryptionLabApp.Data;

public class AppDbContext : DbContext
{
    private readonly IDataProtector _protector;
    private readonly IAesEncryptionService _aes;

    public DbSet<EmployeClair> EmployesClair => Set<EmployeClair>();
    public DbSet<EmployeValueConverter> EmployesValueConverter => Set<EmployeValueConverter>();
    public DbSet<EmployeAes> EmployesAes => Set<EmployeAes>();

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IDataProtectionProvider dataProtection,
        IAesEncryptionService aes)
        : base(options)
    {
        _protector = dataProtection.CreateProtector("DbEncryptionLab.ValueConverter");
        _aes = aes;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Pattern 1: No encryption (clear text) ──
        modelBuilder.Entity<EmployeClair>(entity =>
        {
            entity.ToTable("EmployesClair");
            // No conversion — all data stored as-is
        });

        // ── Pattern 2: Value Converters with Data Protection API ──
        var dpConverter = new ValueConverter<string, string>(
            v => _protector.Protect(v),       // Write → encrypt
            v => _protector.Unprotect(v)      // Read  → decrypt
        );

        modelBuilder.Entity<EmployeValueConverter>(entity =>
        {
            entity.ToTable("EmployesValueConverter");
            entity.Property(e => e.NumeroNational).HasConversion(dpConverter);
            entity.Property(e => e.Salaire).HasConversion(dpConverter);
            entity.Property(e => e.CompteBancaire).HasConversion(dpConverter);
            // Nom, Prenom, Email remain in clear
        });

        // ── Pattern 3: AES-256-CBC with HMAC (manual encryption) ──
        var aesConverter = new ValueConverter<string, string>(
            v => _aes.Encrypt(v),
            v => _aes.Decrypt(v)
        );

        modelBuilder.Entity<EmployeAes>(entity =>
        {
            entity.ToTable("EmployesAes");
            entity.Property(e => e.NumeroNational).HasConversion(aesConverter);
            entity.Property(e => e.Salaire).HasConversion(aesConverter);
            entity.Property(e => e.CompteBancaire).HasConversion(aesConverter);
            // Nom, Prenom, Email remain in clear
        });
    }
}
