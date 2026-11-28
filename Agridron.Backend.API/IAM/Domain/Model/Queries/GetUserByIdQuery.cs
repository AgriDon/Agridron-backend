namespace Agridron.Backend.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to find a user by its database identifier.
/// </summary>
/// <param name="Id">The user identifier.</param>
public sealed record GetUserByIdQuery(long Id);
