namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>Contract for password hashing and hash verification.</summary>
public interface IHashingService
{
    /// <summary>Generates a salted hash for a plain-text password.</summary>
    string HashPassword(string password);

    /// <summary>Verifies a plain-text password against a stored hash.</summary>
    bool VerifyPassword(string password, string passwordHash);
}
