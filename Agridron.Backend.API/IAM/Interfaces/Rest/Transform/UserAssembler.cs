using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that transforms <see cref="User" /> aggregates into <see cref="UserResource" /> API responses.
/// </summary>
/// <remarks>
///     Design Pattern: Assembler / DTO — performs explicit, reflection-free translation from domain entities to REST DTOs.
/// </remarks>
internal static class UserAssembler
{
    /// <summary>
    ///     Maps a <see cref="User" /> aggregate to a <see cref="UserResource" />.
    /// </summary>
    /// <param name="user">The source user aggregate.</param>
    /// <returns>The mapped <see cref="UserResource" />.</returns>
    public static UserResource ToResourceFrom(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserResource(
            user.Id.Value,
            user.FullName,
            user.Email.Value,
            user.Role.ToString(),
            user.PhoneNumber,
            user.Status.ToString(),
            user.EmailVerified,
            user.CreatedAt);
    }
}
