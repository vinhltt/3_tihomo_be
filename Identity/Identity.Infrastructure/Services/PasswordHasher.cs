using Identity.Application.Common.Interfaces;

namespace Identity.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly int _workFactor = 12; // BCrypt work factor

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}