namespace Agridron.Backend.API.IAM.Interfaces.ACL;

/// <summary>
///     Read-only user snapshot exposed to external bounded contexts across the IAM Anti-Corruption Layer.
/// </summary>
/// <remarks>
///     Design Pattern: Anti-Corruption Layer (ACL) / DTO — prevents other bounded contexts from depending
///     directly on the internal <c>User</c> aggregate or its value objects.
/// </remarks>
/// <param name="UserId">The database identifier of the user.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Email">The user's normalized email address.</param>
/// <param name="Role">The user's assigned role name.</param>
/// <param name="IsActive">Indicates whether the user account is currently active.</param>
/// <param name="EmailVerified">Indicates whether the user's email address has been verified.</param>
public sealed record UserSnapshotDto(
    long UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    bool EmailVerified);
