namespace DataProtectionLabApp.Models;

public class DatabaseEncryptionViewModel
{
    public List<Patient> Patients { get; set; } = [];

    // Raw rows as stored in SQLite (encrypted values)
    public List<Dictionary<string, string>> RawRows { get; set; } = [];
}
