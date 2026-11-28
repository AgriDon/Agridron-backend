namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to register a new user account (AU-001).
/// </summary>
/// <param name="FullName">The user's full name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The raw password to be hashed before persistence.</param>
/// <param name="PhoneNumber">An optional contact phone number.</param>
/// <param name="Role">An optional role name (defaults to <c>Farmer</c>).</param>
public sealed record SignUpCommand(
    string FullName,
    string Email,
    string Password,
    string? PhoneNumber,
    string? Role = null);
