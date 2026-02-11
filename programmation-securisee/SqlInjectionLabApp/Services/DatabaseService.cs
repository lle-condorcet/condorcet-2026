using Microsoft.Data.Sqlite;
using SqlInjectionLabApp.Models;

namespace SqlInjectionLabApp.Services;

/// <summary>
/// Service de base de donnees SQLite avec methodes vulnerables et securisees.
/// La BDD est creee en memoire et reinitialisee a chaque demarrage.
/// </summary>
public class DatabaseService : IDisposable
{
    private readonly SqliteConnection _connection;

    public DatabaseService()
    {
        // Connexion en memoire partagee (persiste tant que l'app tourne)
        _connection = new SqliteConnection("Data Source=lab_database;Mode=Memory;Cache=Shared");
        _connection.Open();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var cmd = _connection.CreateCommand();

        // Table des personnes (donnees personnelles sensibles)
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Persons (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FirstName TEXT NOT NULL,
                LastName TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT,
                Address TEXT,
                City TEXT,
                NationalId TEXT,
                DateOfBirth TEXT,
                CreditCard TEXT,
                Iban TEXT,
                Salary REAL,
                Department TEXT
            );
        """;
        cmd.ExecuteNonQuery();

        // Table des comptes utilisateurs (avec mots de passe en clair = vulnerable)
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL,
                Email TEXT
            );
        """;
        cmd.ExecuteNonQuery();

        // Table secrete (simule une table admin cachee)
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS SecretConfig (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
        """;
        cmd.ExecuteNonQuery();

        SeedData();
    }

    private void SeedData()
    {
        // Verifier si les donnees existent deja
        using var checkCmd = _connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Persons";
        var count = (long)(checkCmd.ExecuteScalar() ?? 0);
        if (count > 0) return;

        // Donnees personnelles simulees (belges)
        var persons = new[]
        {
            ("Jean", "Dupont", "jean.dupont@mail.be", "+32 471 23 45 67", "Rue de la Loi 42", "Bruxelles", "85.07.15-123.45", "1985-07-15", "4532-XXXX-XXXX-8901", "BE68 5390 0754 7034", 3250.00m, "Comptabilite"),
            ("Marie", "Lecomte", "marie.lecomte@mail.be", "+32 485 67 89 01", "Avenue Louise 156", "Bruxelles", "90.03.22-456.78", "1990-03-22", "5412-XXXX-XXXX-3456", "BE71 0961 2345 6789", 4100.00m, "Direction"),
            ("Pierre", "Vandenbroeck", "pierre.vdb@mail.be", "+32 476 11 22 33", "Grote Markt 8", "Antwerpen", "78.11.30-789.01", "1978-11-30", "4916-XXXX-XXXX-7890", "BE43 0689 9999 0023", 2800.00m, "IT"),
            ("Sophie", "Martin", "sophie.martin@mail.be", "+32 492 44 55 66", "Rue Hors-Chateau 12", "Liege", "95.06.08-234.56", "1995-06-08", "4539-XXXX-XXXX-1234", "BE09 7340 5577 1136", 3500.00m, "RH"),
            ("Luc", "Dubois", "luc.dubois@mail.be", "+32 478 77 88 99", "Veldstraat 45", "Gent", "82.12.01-567.89", "1982-12-01", "5425-XXXX-XXXX-5678", "BE62 5100 0754 7061", 3100.00m, "Comptabilite"),
            ("Isabelle", "Claessens", "isabelle.c@mail.be", "+32 486 00 11 22", "Boulevard de la Sauveniere 7", "Liege", "88.09.14-890.12", "1988-09-14", "4556-XXXX-XXXX-9012", "BE15 2100 0654 3178", 4500.00m, "Direction"),
            ("Thomas", "De Smedt", "thomas.ds@mail.be", "+32 473 33 44 55", "Meir 100", "Antwerpen", "92.01.25-345.67", "1992-01-25", "4916-XXXX-XXXX-3456", "BE38 0017 5469 8213", 2950.00m, "IT"),
            ("Amelie", "Fontaine", "amelie.f@mail.be", "+32 495 66 77 88", "Rue du Midi 23", "Charleroi", "97.04.17-678.90", "1997-04-17", "5412-XXXX-XXXX-7890", "BE55 7330 2145 3697", 2700.00m, "Support"),
            ("Nicolas", "Janssens", "nicolas.j@mail.be", "+32 477 99 00 11", "Grand Place 1", "Mons", "80.08.03-901.23", "1980-08-03", "4532-XXXX-XXXX-1234", "BE82 6528 3177 4092", 3800.00m, "IT"),
            ("Caroline", "Hermans", "caroline.h@mail.be", "+32 489 22 33 44", "Bondgenotenlaan 50", "Leuven", "93.02.28-012.34", "1993-02-28", "4539-XXXX-XXXX-5678", "BE91 7512 0489 5631", 3400.00m, "RH"),
        };

        foreach (var p in persons)
        {
            using var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = """
                INSERT INTO Persons (FirstName, LastName, Email, Phone, Address, City, NationalId, DateOfBirth, CreditCard, Iban, Salary, Department)
                VALUES ($fn, $ln, $email, $phone, $addr, $city, $nid, $dob, $cc, $iban, $sal, $dept)
            """;
            insertCmd.Parameters.AddWithValue("$fn", p.Item1);
            insertCmd.Parameters.AddWithValue("$ln", p.Item2);
            insertCmd.Parameters.AddWithValue("$email", p.Item3);
            insertCmd.Parameters.AddWithValue("$phone", p.Item4);
            insertCmd.Parameters.AddWithValue("$addr", p.Item5);
            insertCmd.Parameters.AddWithValue("$city", p.Item6);
            insertCmd.Parameters.AddWithValue("$nid", p.Item7);
            insertCmd.Parameters.AddWithValue("$dob", p.Item8);
            insertCmd.Parameters.AddWithValue("$cc", p.Item9);
            insertCmd.Parameters.AddWithValue("$iban", p.Item10);
            insertCmd.Parameters.AddWithValue("$sal", p.Item11);
            insertCmd.Parameters.AddWithValue("$dept", p.Item12);
            insertCmd.ExecuteNonQuery();
        }

        // Comptes utilisateurs
        var users = new[]
        {
            ("admin", "Admin@2026!", "admin", "admin@condorcet.be"),
            ("jean.dupont", "P@ssword1", "user", "jean.dupont@mail.be"),
            ("marie.lecomte", "Marie1990!", "manager", "marie.lecomte@mail.be"),
            ("dba_backup", "Sup3rS3cret!", "admin", "dba@condorcet.be"),
        };

        foreach (var u in users)
        {
            using var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = """
                INSERT INTO Users (Username, Password, Role, Email)
                VALUES ($user, $pass, $role, $email)
            """;
            insertCmd.Parameters.AddWithValue("$user", u.Item1);
            insertCmd.Parameters.AddWithValue("$pass", u.Item2);
            insertCmd.Parameters.AddWithValue("$role", u.Item3);
            insertCmd.Parameters.AddWithValue("$email", u.Item4);
            insertCmd.ExecuteNonQuery();
        }

        // Donnees secretes
        var secrets = new[]
        {
            ("db_connection_string", "Server=prod-db.condorcet.internal;Database=HR_Prod;User=sa;Password=Pr0d_Passw0rd!"),
            ("api_key_payment", "sk_live_4eC39HqLyjWDarjtT1zdp7dc"),
            ("jwt_secret", "eyJhbGciOiJIUzI1NiJ9.c2VjcmV0X2tleQ"),
            ("backup_encryption_key", "AES256:k3y_f0r_b4ckup_2026!"),
        };

        foreach (var s in secrets)
        {
            using var insertCmd = _connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO SecretConfig (Key, Value) VALUES ($key, $val)";
            insertCmd.Parameters.AddWithValue("$key", s.Item1);
            insertCmd.Parameters.AddWithValue("$val", s.Item2);
            insertCmd.ExecuteNonQuery();
        }
    }

    // ================================================================
    // RECHERCHE VULNERABLE - concatenation de chaines = SQL Injection
    // ================================================================
    public (List<Person> results, string query, string? error) SearchVulnerable(string searchTerm)
    {
        // DANGEREUX : le terme de recherche est injecte directement dans la requete
        var sql = $"SELECT * FROM Persons WHERE LastName LIKE '%{searchTerm}%' OR City LIKE '%{searchTerm}%'";

        var results = new List<Person>();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(ReadPerson(reader));
            }
            return (results, sql, null);
        }
        catch (Exception ex)
        {
            return (results, sql, ex.Message);
        }
    }

    // ================================================================
    // RECHERCHE SECURISEE - requetes parametrees
    // ================================================================
    public (List<Person> results, string query, string? error) SearchSecure(string searchTerm)
    {
        var sql = "SELECT * FROM Persons WHERE LastName LIKE $term OR City LIKE $term";
        var displaySql = $"SELECT * FROM Persons WHERE LastName LIKE @term OR City LIKE @term  -- @term = '%{searchTerm}%'";

        var results = new List<Person>();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$term", $"%{searchTerm}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(ReadPerson(reader));
            }
            return (results, displaySql, null);
        }
        catch (Exception ex)
        {
            return (results, displaySql, ex.Message);
        }
    }

    // ================================================================
    // LOGIN VULNERABLE
    // ================================================================
    public (UserAccount? user, string query, string? error) LoginVulnerable(string username, string password)
    {
        var sql = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";

        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var user = new UserAccount
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3),
                    Email = reader.GetString(4),
                };
                return (user, sql, null);
            }
            return (null, sql, null);
        }
        catch (Exception ex)
        {
            return (null, sql, ex.Message);
        }
    }

    // ================================================================
    // LOGIN SECURISE
    // ================================================================
    public (UserAccount? user, string query, string? error) LoginSecure(string username, string password)
    {
        var sql = "SELECT * FROM Users WHERE Username = $user AND Password = $pass";
        var displaySql = $"SELECT * FROM Users WHERE Username = @user AND Password = @pass  -- @user = '{username}'";

        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("$user", username);
            cmd.Parameters.AddWithValue("$pass", password);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var user = new UserAccount
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    Role = reader.GetString(3),
                    Email = reader.GetString(4),
                };
                return (user, displaySql, null);
            }
            return (null, displaySql, null);
        }
        catch (Exception ex)
        {
            return (null, displaySql, ex.Message);
        }
    }

    // ================================================================
    // REQUETE LIBRE VULNERABLE (pour les exercices avances)
    // ================================================================
    public (List<Dictionary<string, object?>> results, string? error) ExecuteRawQuery(string sql)
    {
        var results = new List<Dictionary<string, object?>>();
        try
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }
            return (results, null);
        }
        catch (Exception ex)
        {
            return (results, ex.Message);
        }
    }

    /// <summary>
    /// Reinitialise la BDD (supprime et recree les tables).
    /// </summary>
    public void ResetDatabase()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            DROP TABLE IF EXISTS Persons;
            DROP TABLE IF EXISTS Users;
            DROP TABLE IF EXISTS SecretConfig;
        """;
        cmd.ExecuteNonQuery();
        InitializeDatabase();
    }

    private static Person ReadPerson(SqliteDataReader reader)
    {
        return new Person
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString(reader.GetOrdinal("Phone")),
            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString(reader.GetOrdinal("Address")),
            City = reader.IsDBNull(reader.GetOrdinal("City")) ? "" : reader.GetString(reader.GetOrdinal("City")),
            NationalId = reader.IsDBNull(reader.GetOrdinal("NationalId")) ? "" : reader.GetString(reader.GetOrdinal("NationalId")),
            DateOfBirth = reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) ? "" : reader.GetString(reader.GetOrdinal("DateOfBirth")),
            CreditCard = reader.IsDBNull(reader.GetOrdinal("CreditCard")) ? "" : reader.GetString(reader.GetOrdinal("CreditCard")),
            Iban = reader.IsDBNull(reader.GetOrdinal("Iban")) ? "" : reader.GetString(reader.GetOrdinal("Iban")),
            Salary = reader.IsDBNull(reader.GetOrdinal("Salary")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Salary")),
            Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? "" : reader.GetString(reader.GetOrdinal("Department")),
        };
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
