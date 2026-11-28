using Agridron.Backend.API.IAM.Domain.Model.Aggregates;

namespace Agridron.Backend.API.IAM.Application.Internal.OutboundServices;

/// <summary>Contract for issuing signed authentication tokens.</summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT containing identity and role claims for the given user.</summary>
    string GenerateToken(User user);
}
