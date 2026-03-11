using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataProtectionLabApp.Data;
using DataProtectionLabApp.Models;

namespace DataProtectionLabApp.Controllers;

public class DatabaseEncryptionController : Controller
{
    private readonly EncryptedDbContext _db;

    public DatabaseEncryptionController(EncryptedDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // 1. Get patients via EF Core (automatically decrypted by value converter)
        var patients = await _db.Patients.ToListAsync();

        // 2. Read raw data directly from SQLite to show what the DB actually stores
        var rawRows = new List<Dictionary<string, string>>();

        var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Nom, Prenom, NumeroNational, Diagnostic, NoteMedicale FROM Patients";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                rawRows.Add(new Dictionary<string, string>
                {
                    ["Id"] = reader["Id"].ToString()!,
                    ["Nom"] = reader["Nom"].ToString()!,
                    ["Prenom"] = reader["Prenom"].ToString()!,
                    ["NumeroNational"] = reader["NumeroNational"].ToString()!,
                    ["Diagnostic"] = reader["Diagnostic"].ToString()!,
                    ["NoteMedicale"] = reader["NoteMedicale"].ToString()!,
                });
            }
        }
        finally
        {
            await conn.CloseAsync();
        }

        var vm = new DatabaseEncryptionViewModel
        {
            Patients = patients,
            RawRows = rawRows
        };

        return View(vm);
    }
}
