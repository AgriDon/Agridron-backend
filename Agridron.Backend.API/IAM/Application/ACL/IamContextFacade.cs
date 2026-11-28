using Agridron.Backend.API.IAM.Application.QueryServices;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.IAM.Interfaces.ACL;

namespace Agridron.Backend.API.IAM.Application.ACL;

/// <summary>ACL facade translating IAM user queries into cross-context DTOs.</summary>
public sealed class IamContextFacade(IUserQueryService userQueryService) : IIamContextFacade
{
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

    public async Task<bool> ExistsActiveUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var result = await userQueryService.Handle(new GetUserByIdQuery(userId), cancellationToken);
        return result.IsSuccess && result.Value is not null && result.Value.IsActive;
    }

    public async Task<long?> FetchUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await userQueryService.Handle(new GetUserByEmailQuery(email), cancellationToken);
        return result.IsSuccess && result.Value is not null ? result.Value.Id.Value : null;
    }
}
