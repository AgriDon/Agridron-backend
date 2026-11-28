namespace Agridron.Backend.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Lifecycle status of a user account.
/// </summary>
public enum UserStatus
{
    /// <summary>The account is active and can authenticate.</summary>
    Active = 0,

    /// <summary>The account is inactive and cannot authenticate.</summary>
    Inactive = 1
}
