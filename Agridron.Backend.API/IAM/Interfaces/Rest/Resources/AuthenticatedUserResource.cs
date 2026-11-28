namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Response DTO returned after successful authentication.
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — decouples HTTP response contracts from internal domain aggregates.
/// </remarks>
/// <param name="Id">The authenticated user's database identifier.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Email">The user's normalized email address.</param>
/// <param name="Role">The user's assigned role name.</param>
/// <param name="EmailVerified">Indicates whether the user's email address has been verified.</param>
/// <param name="Token">The signed JWT Bearer token.</param>
public sealed record AuthenticatedUserResource(
    long Id,
    string FullName,
    string Email,
    string Role,
    bool EmailVerified,
    string Token);
