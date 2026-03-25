using Microsoft.EntityFrameworkCore;
using ElkLoggingLabApp.Models;

namespace ElkLoggingLabApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Pro X1", Description = "Ordinateur portable haute performance", Price = 1299.99m, Category = "Electronique", CreatedAt = DateTime.Parse("2025-01-15"), UpdatedAt = DateTime.Parse("2025-01-15") },
            new Product { Id = 2, Name = "Clavier Mecanique RGB", Description = "Clavier gaming switches Cherry MX", Price = 149.99m, Category = "Peripheriques", CreatedAt = DateTime.Parse("2025-02-01"), UpdatedAt = DateTime.Parse("2025-02-01") },
            new Product { Id = 3, Name = "Ecran 4K 27 pouces", Description = "Moniteur IPS haute resolution", Price = 549.99m, Category = "Electronique", CreatedAt = DateTime.Parse("2025-02-10"), UpdatedAt = DateTime.Parse("2025-02-10") },
            new Product { Id = 4, Name = "Souris Ergonomique", Description = "Souris verticale sans fil", Price = 79.99m, Category = "Peripheriques", CreatedAt = DateTime.Parse("2025-03-01"), UpdatedAt = DateTime.Parse("2025-03-01") },
            new Product { Id = 5, Name = "Casque Audio Pro", Description = "Casque a reduction de bruit active", Price = 349.99m, Category = "Audio", CreatedAt = DateTime.Parse("2025-03-15"), UpdatedAt = DateTime.Parse("2025-03-15") },
            new Product { Id = 6, Name = "Webcam HD 1080p", Description = "Camera USB avec micro integre", Price = 89.99m, Category = "Peripheriques", CreatedAt = DateTime.Parse("2025-04-01"), UpdatedAt = DateTime.Parse("2025-04-01") },
            new Product { Id = 7, Name = "Disque SSD 1To", Description = "SSD NVMe haute vitesse", Price = 119.99m, Category = "Stockage", CreatedAt = DateTime.Parse("2025-04-15"), UpdatedAt = DateTime.Parse("2025-04-15") },
            new Product { Id = 8, Name = "Hub USB-C", Description = "Station d'accueil multiport", Price = 69.99m, Category = "Accessoires", CreatedAt = DateTime.Parse("2025-05-01"), UpdatedAt = DateTime.Parse("2025-05-01") }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, ProductId = 1, Quantity = 1, TotalPrice = 1299.99m, CustomerEmail = "alice@example.com", Status = "Completed", CreatedAt = DateTime.Parse("2025-06-01") },
            new Order { Id = 2, ProductId = 2, Quantity = 2, TotalPrice = 299.98m, CustomerEmail = "bob@example.com", Status = "Completed", CreatedAt = DateTime.Parse("2025-06-05") },
            new Order { Id = 3, ProductId = 5, Quantity = 1, TotalPrice = 349.99m, CustomerEmail = "charlie@example.com", Status = "Pending", CreatedAt = DateTime.Parse("2025-06-10") }
        );

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Product)
            .WithMany()
            .HasForeignKey(o => o.ProductId);
    }
}
