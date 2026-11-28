namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

public sealed record UpdateUserProfileCommand(
    long UserId,
    string FullName,
    string? PhoneNumber);
