using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

internal static class UserAssembler
{
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
