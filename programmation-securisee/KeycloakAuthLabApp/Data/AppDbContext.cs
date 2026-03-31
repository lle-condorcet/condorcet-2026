using Microsoft.EntityFrameworkCore;
using KeycloakAuthLabApp.Models;

namespace KeycloakAuthLabApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<SecureDocument> Documents => Set<SecureDocument>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(e => e.KeycloakId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<SecureDocument>(entity =>
        {
            entity.HasIndex(e => e.Classification);
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });

        // Seed data - documents with different classification levels
        modelBuilder.Entity<SecureDocument>().HasData(
            new SecureDocument
            {
                Id = 1,
                Title = "Politique de bienvenue",
                Content = "Bienvenue dans l'application securisee. Ce document est accessible a tous les utilisateurs authentifies.",
                Classification = Classification.Public,
                OwnerId = "admin",
                CreatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc)
            },
            new SecureDocument
            {
                Id = 2,
                Title = "Guide des procedures internes",
                Content = "Ce document decrit les procedures internes de l'entreprise. Acces reserve aux employes.",
                Classification = Classification.Internal,
                OwnerId = "manager",
                CreatedAt = new DateTime(2026, 2, 1, 14, 30, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 2, 1, 14, 30, 0, DateTimeKind.Utc)
            },
            new SecureDocument
            {
                Id = 3,
                Title = "Rapport financier Q1 2026",
                Content = "Chiffre d'affaires: 2.5M EUR. Marge operationnelle: 18%. Details des investissements strategiques.",
                Classification = Classification.Confidential,
                OwnerId = "manager",
                CreatedAt = new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 10, 16, 45, 0, DateTimeKind.Utc)
            },
            new SecureDocument
            {
                Id = 4,
                Title = "Plan de securite infrastructure",
                Content = "Architecture reseau detaillee. Adresses IP internes: 10.0.0.0/8. Credentials des services critiques. Plan de reponse aux incidents.",
                Classification = Classification.Secret,
                OwnerId = "admin",
                CreatedAt = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 20, 11, 0, 0, DateTimeKind.Utc)
            },
            new SecureDocument
            {
                Id = 5,
                Title = "Notes de reunion equipe",
                Content = "Reunion du 15 mars. Sujets abordes: planning sprint, retrospective, ameliorations continues.",
                Classification = Classification.Internal,
                OwnerId = "user",
                CreatedAt = new DateTime(2026, 3, 15, 15, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 15, 15, 0, 0, DateTimeKind.Utc)
            },
            new SecureDocument
            {
                Id = 6,
                Title = "Audit de securite - Resultats",
                Content = "Vulnerabilites identifiees: 3 critiques, 7 moyennes, 12 faibles. Plan de remediation en cours.",
                Classification = Classification.Secret,
                OwnerId = "admin",
                CreatedAt = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 3, 5, 14, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
