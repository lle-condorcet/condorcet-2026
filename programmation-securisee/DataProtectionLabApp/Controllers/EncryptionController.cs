using DataProtectionLabApp.Models;
using DataProtectionLabApp.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace DataProtectionLabApp.Controllers;

public class EncryptionController : Controller
{
    private readonly IDataProtector _protector;
    private readonly ITimeLimitedDataProtector _timeLimitedProtector;
    private readonly SensitiveDataStore _store;

    public EncryptionController(IDataProtectionProvider provider, SensitiveDataStore store)
    {
        _protector = provider.CreateProtector("DataProtectionLab.SensitiveData");
        _timeLimitedProtector = _protector.ToTimeLimitedDataProtector();
        _store = store;
    }

    public IActionResult Vulnerable()
    {
        var records = _store.GetAllRecords();

        var model = new EncryptionViewModel
        {
            Records = records,
            IsEncrypted = false
        };

        return View(model);
    }

    public IActionResult Secure()
    {
        var records = _store.GetAllRecords();
        foreach (var record in records)
        {
            record.EncryptedValue = _protector.Protect(record.Value);
        }

        var model = new EncryptionViewModel
        {
            Records = records,
            IsEncrypted = true
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Encrypt(string plainText)
    {
        var records = _store.GetAllRecords();
        foreach (var record in records)
        {
            record.EncryptedValue = _protector.Protect(record.Value);
        }

        string? cipherText = null;
        if (!string.IsNullOrWhiteSpace(plainText))
        {
            cipherText = _protector.Protect(plainText);
        }

        var model = new EncryptionViewModel
        {
            Records = records,
            IsEncrypted = true,
            PlainText = plainText,
            CipherText = cipherText
        };

        return View("Secure", model);
    }

    [HttpPost]
    public IActionResult EncryptTimeLimited(string plainText, int lifetimeSeconds = 30)
    {
        var records = _store.GetAllRecords();
        foreach (var record in records)
        {
            record.EncryptedValue = _protector.Protect(record.Value);
        }

        string? cipher = null;
        DateTime? expiration = null;
        if (!string.IsNullOrWhiteSpace(plainText))
        {
            var lifetime = TimeSpan.FromSeconds(lifetimeSeconds);
            cipher = _timeLimitedProtector.Protect(plainText, lifetime);
            expiration = DateTime.Now.Add(lifetime);
        }

        var model = new EncryptionViewModel
        {
            Records = records,
            IsEncrypted = true,
            PlainText = plainText,
            TimeLimitedCipher = cipher,
            Expiration = expiration
        };

        return View("Secure", model);
    }

    [HttpPost]
    public IActionResult Decrypt(string cipherText)
    {
        var records = _store.GetAllRecords();
        foreach (var record in records)
        {
            record.EncryptedValue = _protector.Protect(record.Value);
        }

        string? plainText = null;
        string? error = null;
        try
        {
            plainText = _protector.Unprotect(cipherText);
        }
        catch (Exception ex)
        {
            error = $"Erreur de dechiffrement : {ex.Message}";
        }

        var model = new EncryptionViewModel
        {
            Records = records,
            IsEncrypted = true,
            PlainText = plainText ?? error,
            CipherText = cipherText
        };

        return View("Secure", model);
    }
}
