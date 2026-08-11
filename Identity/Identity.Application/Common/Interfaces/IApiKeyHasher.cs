namespace Identity.Application.Common.Interfaces;

public interface IApiKeyHasher
{
    string GenerateApiKey();
    string HashApiKey(string plainKey);
    bool VerifyApiKey(string plainKey, string hashedKey);
}