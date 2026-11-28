using Agridron.Backend.API.Shared.Domain.Model.Events;

namespace Agridron.Backend.API.IAM.Domain.Model.Events;

public sealed record UserRegisteredEvent(
    long UserId,
    string FullName,
    string Email,
    string Role,
    DateTimeOffset OccurredAt) : IEvent;
