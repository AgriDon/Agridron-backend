namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Outbound service contract for adaptive password hashing and verification.
/// </summary>
/// <remarks>
///     Design Pattern: Strategy — defines an interchangeable password-hashing algorithm contract
///     so the application layer remains decoupled from BCrypt or any specific cryptographic library.
/// </remarks>
public interface IHashingService
{
    /// <summary>
    ///     Generates a salted cryptographic hash for a raw password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The salted password hash string.</returns>
    string HashPassword(string password);

    /// <summary>
    ///     Verifies a raw password against a stored password hash.
    /// </summary>
    /// <param name="password">The plain-text password provided by the user.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
