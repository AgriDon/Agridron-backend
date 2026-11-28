using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that transforms a <see cref="SignUpResource" /> into a <see cref="SignUpCommand" />.
/// </summary>
/// <remarks>
///     Design Pattern: Assembler / DTO — translates HTTP request payloads into domain commands.
/// </remarks>
public static class SignUpCommandFromResourceAssembler
{
    /// <summary>
    ///     Maps a <see cref="SignUpResource" /> to a <see cref="SignUpCommand" />.
    /// </summary>
    /// <param name="resource">The incoming sign-up request resource.</param>
    /// <returns>The constructed <see cref="SignUpCommand" />.</returns>
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new SignUpCommand(
            resource.FullName,
            resource.Email,
            resource.Password,
            resource.PhoneNumber,
            resource.Role);
    }
}
