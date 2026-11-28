using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

/// <summary>
///     Assembler that transforms a <see cref="SignInResource" /> into a <see cref="SignInCommand" />.
/// </summary>
/// <remarks>
///     Design Pattern: Assembler / DTO — translates HTTP sign-in payloads into domain commands.
/// </remarks>
public static class SignInCommandFromResourceAssembler
{
    /// <summary>
    ///     Maps a <see cref="SignInResource" /> to a <see cref="SignInCommand" />.
    /// </summary>
    /// <param name="resource">The incoming sign-in request resource.</param>
    /// <returns>The constructed <see cref="SignInCommand" />.</returns>
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new SignInCommand(resource.Email, resource.Password);
    }
}
