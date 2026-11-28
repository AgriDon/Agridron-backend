namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to update basic profile information for an existing user (AU-005).
/// </summary>
/// <param name="UserId">The identifier of the user to update.</param>
/// <param name="FullName">The updated full name.</param>
/// <param name="PhoneNumber">The updated optional phone number.</param>
public sealed record UpdateUserProfileCommand(
    long UserId,
    string FullName,
    string? PhoneNumber);
