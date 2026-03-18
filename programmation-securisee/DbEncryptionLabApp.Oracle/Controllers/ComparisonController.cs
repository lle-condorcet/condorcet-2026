using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbEncryptionLabApp.Oracle.Data;
using DbEncryptionLabApp.Oracle.Models;

namespace DbEncryptionLabApp.Oracle.Controllers;

public class ComparisonController : Controller
{
    private readonly AppDbContext _db;

    public ComparisonController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var vm = new ComparisonViewModel
        {
            EmployesClair = await _db.EmployesClair.ToListAsync(),
            EmployesValueConverter = await _db.EmployesValueConverter.ToListAsync(),
            EmployesAes = await _db.EmployesAes.ToListAsync(),
            RawRowsClair = await ReadRawRows("EMPLOYES_CLAIR"),
            RawRowsValueConverter = await ReadRawRows("EMPLOYES_VALUE_CONVERTER"),
            RawRowsAes = await ReadRawRows("EMPLOYES_AES"),
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
