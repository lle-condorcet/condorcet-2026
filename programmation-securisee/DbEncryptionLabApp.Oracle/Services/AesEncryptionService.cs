using System.Security.Cryptography;

namespace DbEncryptionLabApp.Oracle.Services;

public interface IAesEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

/// <summary>
/// AES-256-CBC encryption service with HMAC-SHA256 authentication.
/// Format: Base64(IV + CipherText + HMAC)
///
/// This demonstrates manual encryption — giving full control over
/// the algorithm, key management, and encrypted format.
/// </summary>
public class AesEncryptionService : IAesEncryptionService
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hmacKey;

    public AesEncryptionService(IConfiguration configuration)
    {
        // In production: use Oracle Wallet, OKV, or a KMS
        var keyBase64 = configuration["Encryption:AesKey"]
            ?? throw new InvalidOperationException("Encryption:AesKey is not configured");
        var hmacBase64 = configuration["Encryption:HmacKey"]
            ?? throw new InvalidOperationException("Encryption:HmacKey is not configured");

        _encryptionKey = Convert.FromBase64String(keyBase64);
        _hmacKey = Convert.FromBase64String(hmacBase64);

        if (_encryptionKey.Length != 32)
            throw new InvalidOperationException("AES key must be 256 bits (32 bytes)");
        if (_hmacKey.Length != 32)
            throw new InvalidOperationException("HMAC key must be 256 bits (32 bytes)");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV(); // Random IV for each encryption

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Combine IV + CipherText
        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        // Add HMAC for authentication (Encrypt-then-MAC)
        using var hmac = new HMACSHA256(_hmacKey);
        var mac = hmac.ComputeHash(combined);

        var result = new byte[combined.Length + mac.Length];
        Buffer.BlockCopy(combined, 0, result, 0, combined.Length);
        Buffer.BlockCopy(mac, 0, result, combined.Length, mac.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullBytes = Convert.FromBase64String(cipherText);

        // Extract HMAC (last 32 bytes)
        const int hmacLength = 32;
        const int ivLength = 16;

        if (fullBytes.Length < ivLength + hmacLength + 1)
            throw new CryptographicException("Invalid ciphertext length");

        var combined = new byte[fullBytes.Length - hmacLength];
        var storedMac = new byte[hmacLength];
        Buffer.BlockCopy(fullBytes, 0, combined, 0, combined.Length);
        Buffer.BlockCopy(fullBytes, combined.Length, storedMac, 0, hmacLength);

        // Verify HMAC before decrypting (prevent padding oracle attacks)
        using var hmac = new HMACSHA256(_hmacKey);
        var computedMac = hmac.ComputeHash(combined);
        if (!CryptographicOperations.FixedTimeEquals(computedMac, storedMac))
            throw new CryptographicException("HMAC verification failed — data may have been tampered with");

        // Extract IV and ciphertext
        var iv = new byte[ivLength];
        var cipherBytes = new byte[combined.Length - ivLength];
        Buffer.BlockCopy(combined, 0, iv, 0, ivLength);
        Buffer.BlockCopy(combined, ivLength, cipherBytes, 0, cipherBytes.Length);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
