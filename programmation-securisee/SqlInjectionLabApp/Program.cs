using Microsoft.EntityFrameworkCore;
using SqlInjectionLabApp.Data;
using SqlInjectionLabApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// BDD SQLite en memoire via ADO.NET (labo injection classique)
builder.Services.AddSingleton<DatabaseService>();

// BDD SQLite fichier via Entity Framework (labo comparaison EF)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=ef_lab.db"));

var app = builder.Build();

// Initialiser la BDD ADO.NET
app.Services.GetRequiredService<DatabaseService>();

// Initialiser la BDD EF (creer + seed)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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
