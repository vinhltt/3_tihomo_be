using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;

namespace Identity.Infrastructure.Services;

public class ApiKeyHasher : IApiKeyHasher
{
    public string GenerateApiKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public string HashApiKey(string plainKey)
    {
        var bytes = Encoding.UTF8.GetBytes(plainKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyApiKey(string plainKey, string hashedKey)
    {
        var hashOfInput = HashApiKey(plainKey);
        return hashOfInput.Equals(hashedKey, StringComparison.Ordinal);
    }
}