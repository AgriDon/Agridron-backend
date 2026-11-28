using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that transforms an authenticated <see cref="User" /> and JWT token into an <see cref="AuthenticatedUserResource" />.
/// </summary>
/// <remarks>
///     Design Pattern: Assembler / DTO — performs explicit, compile-time-checked mapping without reflection-based mappers.
/// </remarks>
internal static class AuthenticatedUserAssembler
{
    /// <summary>
    ///     Maps a <see cref="User" /> aggregate and issued JWT token to an <see cref="AuthenticatedUserResource" />.
    /// </summary>
    /// <param name="user">The authenticated user aggregate.</param>
    /// <param name="token">The signed JWT Bearer token.</param>
    /// <returns>The mapped <see cref="AuthenticatedUserResource" />.</returns>
    public static AuthenticatedUserResource ToResourceFrom(User user, string token)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return new AuthenticatedUserResource(
            user.Id.Value,
            user.FullName,
            user.Email.Value,
            user.Role.ToString(),
            user.EmailVerified,
            token);
    }
}
