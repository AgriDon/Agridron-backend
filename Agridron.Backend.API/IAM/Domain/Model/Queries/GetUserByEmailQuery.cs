namespace Agridron.Backend.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to find a user by its email address.
/// </summary>
/// <param name="Email">The email address to search for.</param>
public sealed record GetUserByEmailQuery(string Email);
