namespace Agridron.Backend.API.IAM.Interfaces.Rest.Resources;

/// <summary>
///     Public profile DTO representing a registered platform user.
/// </summary>
/// <remarks>
///     Design Pattern: DTO / Resource — prevents domain aggregates and sensitive fields (such as password hashes)
///     from leaking across the REST API boundary.
/// </remarks>
/// <param name="Id">The user's database identifier.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Email">The user's normalized email address.</param>
/// <param name="Role">The user's assigned role name.</param>
/// <param name="PhoneNumber">The optional contact phone number.</param>
/// <param name="Status">The account lifecycle status.</param>
/// <param name="EmailVerified">Indicates whether the user's email address has been verified.</param>
/// <param name="CreatedAt">The UTC timestamp when the account was created.</param>
public sealed record UserResource(
    long Id,
    string FullName,
    string Email,
    string Role,
    string? PhoneNumber,
    string Status,
    bool EmailVerified,
    DateTimeOffset? CreatedAt);
