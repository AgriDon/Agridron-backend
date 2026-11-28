using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using BCrypt.Net;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Agridron.Backend.API.IAM.Infrastructure.Hashing.BCrypt.Services;

internal sealed class HashingService : IHashingService
{
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCryptNet.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            return BCryptNet.Verify(password, passwordHash);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }
}
