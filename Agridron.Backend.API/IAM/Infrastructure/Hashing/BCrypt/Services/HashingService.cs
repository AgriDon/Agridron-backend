using Agridron.Backend.API.IAM.Application.Internal.OutboundServices;
using BCrypt.Net;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Agridron.Backend.API.IAM.Infrastructure.Hashing.BCrypt.Services;

/// <summary>
///     BCrypt implementation of <see cref="IHashingService" /> providing adaptive salted password hashing.
/// </summary>
/// <remarks>
///     Design Pattern: Strategy — concrete password-hashing strategy plugged into IAM via dependency injection.
/// </remarks>
internal sealed class HashingService : IHashingService
{
    /// <inheritdoc />
    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCryptNet.HashPassword(password);
    }

    /// <inheritdoc />
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
