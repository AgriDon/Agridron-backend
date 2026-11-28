namespace Agridron.Backend.API.IAM.Interfaces.ACL;

/// <summary>Read-only user projection exposed across bounded contexts.</summary>
public sealed record UserSnapshotDto(
    long UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    bool EmailVerified);
