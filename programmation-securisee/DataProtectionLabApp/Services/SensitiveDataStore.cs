using DataProtectionLabApp.Models;

namespace DataProtectionLabApp.Services;

public class SensitiveDataStore
{
    private readonly List<SensitiveRecord> _records = new()
    {
        new SensitiveRecord { Id = 1, Label = "Carte bancaire (Alice)", Value = "4539 1488 0343 6467" },
        new SensitiveRecord { Id = 2, Label = "IBAN (Bob)", Value = "BE68 5390 0754 7034" },
        new SensitiveRecord { Id = 3, Label = "Mot de passe (Charlie)", Value = "P@ssw0rd_Charlie_2026!" },
        new SensitiveRecord { Id = 4, Label = "Numero national (Diana)", Value = "85.07.15-123.45" },
        new SensitiveRecord { Id = 5, Label = "Carte bancaire (Eve)", Value = "5425 2334 1095 8790" }
    };

    public List<SensitiveRecord> GetAllRecords() => _records.Select(r => new SensitiveRecord
    {
        Id = r.Id,
        Label = r.Label,
        Value = r.Value,
        EncryptedValue = r.EncryptedValue
    }).ToList();
}
