using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbEncryptionLabApp.Postgres.Data;
using DbEncryptionLabApp.Postgres.Models;

namespace DbEncryptionLabApp.Postgres.Controllers;

public class AesEncryptionController : Controller
{
    private readonly AppDbContext _db;

    public AesEncryptionController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // EF Core decrypts automatically via AES Value Converters
        var employes = await _db.EmployesAes.ToListAsync();

        // Read raw (encrypted) data directly from PostgreSQL
        var rawRows = await ReadRawRows("EmployesAes");

        var vm = new PatternViewModel
        {
            PatternName = "Chiffrement AES-256-CBC manuel avec HMAC-SHA256",
            PatternDescription = "Chiffrement manuel avec controle total sur l'algorithme (AES-256-CBC), " +
                "les cles, et le format. Authentification par HMAC-SHA256 (Encrypt-then-MAC) pour prevenir les attaques par oracle de padding.",
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
