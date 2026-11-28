namespace Agridron.Backend.API.IAM.Domain.Model.Commands;

public sealed record SignInCommand(
    string Email,
    string Password);
