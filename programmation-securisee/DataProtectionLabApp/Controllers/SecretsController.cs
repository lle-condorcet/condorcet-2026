using DataProtectionLabApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataProtectionLabApp.Controllers;

public class SecretsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public SecretsController(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public IActionResult Vulnerable()
    {
        var secrets = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = _configuration["ConnectionStrings:DefaultConnection"] ?? "(non defini)",
            ["ApiKeys:PaymentGateway"] = _configuration["ApiKeys:PaymentGateway"] ?? "(non defini)",
            ["ApiKeys:EmailService"] = _configuration["ApiKeys:EmailService"] ?? "(non defini)",
            ["ApiKeys:GoogleMaps"] = _configuration["ApiKeys:GoogleMaps"] ?? "(non defini)",
            ["JwtSettings:SecretKey"] = _configuration["JwtSettings:SecretKey"] ?? "(non defini)"
        };

        string? appSettingsContent = null;
        var appSettingsPath = Path.Combine(_env.ContentRootPath, "appsettings.json");
        if (System.IO.File.Exists(appSettingsPath))
        {
            appSettingsContent = System.IO.File.ReadAllText(appSettingsPath);
        }

        var model = new SecretsViewModel
        {
            ExposedSecrets = secrets,
            AppSettingsContent = appSettingsContent,
            ConfigSource = "appsettings.json"
        };

        return View(model);
    }

    public IActionResult Secure()
    {
        var secrets = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = _configuration["ConnectionStrings:DefaultConnection"] ?? "(non defini)",
            ["ApiKeys:PaymentGateway"] = _configuration["ApiKeys:PaymentGateway"] ?? "(non defini)",
            ["ApiKeys:EmailService"] = _configuration["ApiKeys:EmailService"] ?? "(non defini)",
            ["ApiKeys:GoogleMaps"] = _configuration["ApiKeys:GoogleMaps"] ?? "(non defini)",
            ["JwtSettings:SecretKey"] = _configuration["JwtSettings:SecretKey"] ?? "(non defini)"
        };

        var masked = new Dictionary<string, string>();
        foreach (var kvp in secrets)
        {
            masked[kvp.Key] = MaskSecret(kvp.Value);
        }

        var userSecretsPath = GetUserSecretsPath();

        var commands = new List<string>
        {
            "dotnet user-secrets init",
            "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"Server=localhost;Database=DevDB;User=dev;Password=DevOnly!\"",
            "dotnet user-secrets set \"ApiKeys:PaymentGateway\" \"sk_test_dev_key_here\"",
            "dotnet user-secrets set \"ApiKeys:EmailService\" \"SG.dev_sendgrid_key\"",
            "dotnet user-secrets set \"ApiKeys:GoogleMaps\" \"AIzaSy_dev_maps_key\"",
            "dotnet user-secrets set \"JwtSettings:SecretKey\" \"DevSecret_JWT_Key_2026\"",
            "dotnet user-secrets list"
        };

        var model = new SecretsViewModel
        {
            ExposedSecrets = secrets,
            MaskedSecrets = masked,
            UserSecretsPath = userSecretsPath,
            Commands = commands,
            ConfigSource = "User Secrets / Environment Variables"
        };

        return View(model);
    }

    private static string MaskSecret(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= 6)
            return "******";

        return value[..3] + new string('*', Math.Min(value.Length - 6, 20)) + value[^3..];
    }

    private string GetUserSecretsPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(userProfile, "Microsoft", "UserSecrets", "<UserSecretsId>", "secrets.json");
        }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".microsoft", "usersecrets", "<UserSecretsId>", "secrets.json");
    }
}
