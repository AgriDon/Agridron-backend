using Agridron.Backend.API.IAM.Domain.Model.Commands;
using Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.IAM.Interfaces.Rest.Transform;

public static class SignUpCommandFromResourceAssembler
{
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
