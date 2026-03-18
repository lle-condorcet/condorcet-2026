using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbEncryptionLabApp.Postgres.Data;
using DbEncryptionLabApp.Postgres.Models;

namespace DbEncryptionLabApp.Postgres.Controllers;

public class ClearTextController : Controller
{
    private readonly AppDbContext _db;

    public ClearTextController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Read via EF Core
        var employes = await _db.EmployesClair.ToListAsync();

        // Read raw data from PostgreSQL to show what's actually stored
        var rawRows = await ReadRawRows("EmployesClair");

        var vm = new PatternViewModel
        {
            PatternName = "Stockage en clair (aucun chiffrement)",
            PatternDescription = "Les donnees sensibles sont stockees telles quelles dans la base de donnees. " +
                "Toute personne ayant acces a la base (DBA, backup, SQL injection) peut lire les donnees.",
            AppRows = employes.Select(e => new Dictionary<string, string>
            {
                ["Id"] = e.Id.ToString(),
                ["Nom"] = e.Nom,
                ["Prenom"] = e.Prenom,
                ["NumeroNational"] = e.NumeroNational,
                ["Salaire"] = e.Salaire,
                ["CompteBancaire"] = e.CompteBancaire,
                ["Email"] = e.Email
            }).ToList(),
            DbRows = rawRows
        };

        return View(vm);
    }

    private async Task<List<Dictionary<string, string>>> ReadRawRows(string tableName)
    {
        var rows = new List<Dictionary<string, string>>();
        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT \"Id\", \"Nom\", \"Prenom\", \"NumeroNational\", \"Salaire\", \"CompteBancaire\", \"Email\" FROM \"{tableName}\"";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rows.Add(new Dictionary<string, string>
                {
                    ["Id"] = reader["Id"].ToString()!,
                    ["Nom"] = reader["Nom"].ToString()!,
                    ["Prenom"] = reader["Prenom"].ToString()!,
                    ["NumeroNational"] = reader["NumeroNational"].ToString()!,
                    ["Salaire"] = reader["Salaire"].ToString()!,
                    ["CompteBancaire"] = reader["CompteBancaire"].ToString()!,
                    ["Email"] = reader["Email"].ToString()!,
                });
            }
        }
        finally
        {
            await conn.CloseAsync();
        }
        return rows;
    }
}
