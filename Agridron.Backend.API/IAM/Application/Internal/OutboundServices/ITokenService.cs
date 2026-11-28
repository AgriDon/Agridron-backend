using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>
///     Outbound service contract for issuing authentication tokens.
/// </summary>
/// <remarks>
///     Design Pattern: Strategy — decouples token generation strategy (currently HMAC-SHA256 JWT)
///     from the application command handlers.
/// </remarks>
public interface ITokenService
{
    /// <summary>
    ///     Generates a signed authentication token containing user identity and role claims.
    /// </summary>
    /// <param name="user">The authenticated <see cref="User" /> aggregate.</param>
    /// <returns>The serialized signed token string.</returns>
    string GenerateToken(User user);
}
