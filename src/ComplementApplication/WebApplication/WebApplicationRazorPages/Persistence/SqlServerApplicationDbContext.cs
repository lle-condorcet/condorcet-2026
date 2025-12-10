using Microsoft.EntityFrameworkCore;

namespace WebApplicationRazorPages.Persistence;

public class SqlServerApplicationDbContext : DbContext
{
    public SqlServerApplicationDbContext(DbContextOptions<SqlServerApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}