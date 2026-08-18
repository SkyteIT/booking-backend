using System.Security.Cryptography;
using System.Text;
using Ube.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;

public class EncryptionService : IEncryptionService
{
    private readonly string _key;

    public EncryptionService(IConfiguration config)
    {
        _key = config["EncryptionKey:Key"]
            ?? throw new InvalidOperationException("EncryptionKey is not configured.");
    }

    private byte[] GetKeyBytes()
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(_key));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();

        aes.Key = GetKeyBytes();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = GetKeyBytes();

        var iv = fullCipher.Take(16).ToArray();
        var cipher = fullCipher.Skip(16).ToArray();

        using var decryptor = aes.CreateDecryptor(aes.Key, iv);

        var decryptedBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

}