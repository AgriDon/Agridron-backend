using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

internal static class AuthenticatedUserAssembler
{
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
