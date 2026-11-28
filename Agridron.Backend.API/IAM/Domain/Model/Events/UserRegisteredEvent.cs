using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

/// <summary>
///     Domain event raised when a new user account has been persisted and its identity is available.
/// </summary>
/// <param name="UserId">The persisted user identifier.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Email">The user's normalized email address.</param>
/// <param name="Role">The assigned role name.</param>
/// <param name="OccurredAt">The UTC timestamp when the registration happened.</param>
public sealed record UserRegisteredEvent(
    long UserId,
    string FullName,
    string Email,
    string Role,
    DateTimeOffset OccurredAt) : IEvent;
