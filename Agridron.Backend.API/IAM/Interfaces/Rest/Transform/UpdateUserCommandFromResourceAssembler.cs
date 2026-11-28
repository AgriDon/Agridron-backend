using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that transforms an <see cref="UpdateUserResource" /> into an <see cref="UpdateUserProfileCommand" />.
/// </summary>
/// <remarks>
///     Design Pattern: Assembler / DTO — combines route identity and HTTP body fields into a domain command.
/// </remarks>
public static class UpdateUserCommandFromResourceAssembler
{
    /// <summary>
    ///     Maps a user identifier and <see cref="UpdateUserResource" /> to an <see cref="UpdateUserProfileCommand" />.
    /// </summary>
    /// <param name="userId">The target user's database identifier.</param>
    /// <param name="resource">The incoming profile update request resource.</param>
    /// <returns>The constructed <see cref="UpdateUserProfileCommand" />.</returns>
    public static UpdateUserProfileCommand ToCommandFromResource(long userId, UpdateUserResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateUserProfileCommand(userId, resource.FullName, resource.PhoneNumber);
    }
}
