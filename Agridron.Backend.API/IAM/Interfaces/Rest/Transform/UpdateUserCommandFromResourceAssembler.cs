using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

public static class UpdateUserCommandFromResourceAssembler
{
    public static UpdateUserProfileCommand ToCommandFromResource(long userId, UpdateUserResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return new UpdateUserProfileCommand(userId, resource.FullName, resource.PhoneNumber);
    }
}
