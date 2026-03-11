using Microsoft.EntityFrameworkCore;
using DataProtectionLabApp.Data;
using DataProtectionLabApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<DataProtectionLabApp.Services.SensitiveDataStore>();

builder.Services.AddDbContext<EncryptedDbContext>(options =>
    options.UseSqlite("Data Source=dataprotection_lab.db"));

var app = builder.Build();

// Create and seed the database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EncryptedDbContext>();
    db.Database.EnsureCreated();

    if (!db.Patients.Any())
    {
        db.Patients.AddRange(
            new Patient { Nom = "Dupont", Prenom = "Alice", NumeroNational = "85.07.15-123.45", Diagnostic = "Hypertension arterielle", NoteMedicale = "Traitement: Amlodipine 5mg/jour" },
            new Patient { Nom = "Martin", Prenom = "Bob", NumeroNational = "90.03.22-456.78", Diagnostic = "Diabete de type 2", NoteMedicale = "HbA1c: 7.2% - Metformine 850mg x2" },
            new Patient { Nom = "Leroy", Prenom = "Claire", NumeroNational = "78.11.30-789.01", Diagnostic = "Asthme chronique", NoteMedicale = "Ventoline PRN + Symbicort 200/6" }
        );
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
