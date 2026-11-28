namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to record an authenticated user sign-out (AU-007).
/// </summary>
/// <param name="UserId">The authenticated user's database identifier.</param>
public sealed record SignOutCommand(long UserId);
