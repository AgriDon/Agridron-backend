namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

public sealed record SignUpCommand(
    string FullName,
    string Email,
    string Password,
    string? PhoneNumber,
    string? Role = null);
