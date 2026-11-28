using Agridron.Backend.API.IAM.Application.QueryServices;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.IAM.Interfaces.ACL;

namespace Agridron.Backend.API.IAM.Application.ACL;

/// <summary>
///     Implementation of the IAM Anti-Corruption Layer facade.
/// </summary>
/// <remarks>
///     Design Pattern: Facade &amp; Anti-Corruption Layer (ACL) — translates internal IAM query results
///     into lightweight <see cref="UserSnapshotDto" /> projections for consuming bounded contexts.
/// </remarks>
public sealed class IamContextFacade(IUserQueryService userQueryService) : IIamContextFacade
{
    /// <inheritdoc />
    public async Task<UserSnapshotDto?> FetchUserByIdAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        if (result.IsFailure || result.Value is null)
        {
            return null;
        }

        var user = result.Value;
        return new UserSnapshotDto(
            user.Id.Value,
            user.FullName,
            user.Email.Value,
            user.Role.ToString(),
            user.IsActive,
            user.EmailVerified);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsActiveUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return result.IsSuccess && result.Value is not null && result.Value.IsActive;
    }

    /// <inheritdoc />
    public async Task<long?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await userQueryService.Handle(new GetUserByEmailQuery(email), cancellationToken);
        return result.IsSuccess && result.Value is not null ? result.Value.Id.Value : null;
    }
}
