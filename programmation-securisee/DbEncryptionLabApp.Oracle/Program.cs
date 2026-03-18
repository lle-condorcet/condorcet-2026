using Microsoft.EntityFrameworkCore;
using DbEncryptionLabApp.Oracle.Data;
using DbEncryptionLabApp.Oracle.Models;
using DbEncryptionLabApp.Oracle.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IAesEncryptionService, AesEncryptionService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Create and seed the database with sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Seed identical data into all three tables for comparison
    if (!db.EmployesClair.Any())
    {
        var seedData = new[]
        {
            ("Dupont", "Alice", "85.07.15-123.45", "4250 EUR/mois", "BE68 5390 0754 7034", "alice.dupont@company.be"),
            ("Martin", "Bob", "90.03.22-456.78", "3800 EUR/mois", "BE71 0961 2345 6769", "bob.martin@company.be"),
            ("Leroy", "Claire", "78.11.30-789.01", "5100 EUR/mois", "BE23 1234 5678 9012", "claire.leroy@company.be"),
            ("Janssens", "David", "95.06.08-234.56", "3200 EUR/mois", "BE45 6789 0123 4567", "david.janssens@company.be"),
        };

        foreach (var (nom, prenom, nn, salaire, compte, email) in seedData)
        {
            db.EmployesClair.Add(new EmployeClair
            {
                Nom = nom, Prenom = prenom, NumeroNational = nn,
                Salaire = salaire, CompteBancaire = compte, Email = email
            });
            db.EmployesValueConverter.Add(new EmployeValueConverter
            {
                Nom = nom, Prenom = prenom, NumeroNational = nn,
                Salaire = salaire, CompteBancaire = compte, Email = email
            });
            db.EmployesAes.Add(new EmployeAes
            {
                Nom = nom, Prenom = prenom, NumeroNational = nn,
                Salaire = salaire, CompteBancaire = compte, Email = email
            });
        }

        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
