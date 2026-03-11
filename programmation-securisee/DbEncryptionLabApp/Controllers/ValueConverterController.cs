using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbEncryptionLabApp.Data;
using DbEncryptionLabApp.Models;

namespace DbEncryptionLabApp.Controllers;

public class ValueConverterController : Controller
{
    private readonly AppDbContext _db;

    public ValueConverterController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // EF Core decrypts automatically via Value Converters
        var employes = await _db.EmployesValueConverter.ToListAsync();

        // Read raw (encrypted) data directly from SQL Server
        var rawRows = await ReadRawRows("EmployesValueConverter");

        var vm = new PatternViewModel
        {
            PatternName = "Value Converters EF Core + Data Protection API",
            PatternDescription = "Les donnees sont chiffrees de maniere transparente via les Value Converters d'EF Core. " +
                "L'API Data Protection de .NET gere les cles et le chiffrement (AES-256-CBC avec clés rotatives).",
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
            cmd.CommandText = $"SELECT Id, Nom, Prenom, NumeroNational, Salaire, CompteBancaire, Email FROM [{tableName}]";
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
