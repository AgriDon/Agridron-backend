using Agridron.Backend.API.IAM.Application.QueryServices;
using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Errors;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.IAM.Domain.Model.ValueObjects;
using Agridron.Backend.API.IAM.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Model.Exceptions;

namespace Agridron.Backend.API.IAM.Application.Internal.QueryServices;

/// <summary>
///     Application query service implementing read-only user lookup use cases.
/// </summary>
internal sealed class UserQueryService(IUserRepository userRepository) : IUserQueryService
{
    /// <inheritdoc />
    public async Task<Result<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await userRepository.FindByIdAsync(query.Id, cancellationToken);

        return user is null
            ? Result<User>.Failure(IamErrors.UserNotFound(query.Id))
            : Result<User>.Success(user);
    }

    /// <inheritdoc />
    public async Task<Result<User>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        EmailAddress email;
        try
        {
            email = EmailAddress.Create(query.Email);
        }
        catch (DomainException exception)
        {
            return Result<User>.Failure(exception.Error);
        }

        var user = await userRepository.FindByEmailAsync(email.Value, cancellationToken);

        return user is null
            ? Result<User>.Failure(IamErrors.UserNotFoundByEmail(email.Value))
            : Result<User>.Success(user);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<User>>> Handle(
        GetAllUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var users = (await userRepository.ListAsync(cancellationToken))
            .OrderBy(u => u.Id.Value)
            .ToList();

        var pageItems = query.Pagination.IsPaged
            ? users.Skip(query.Pagination.Skip).Take(query.Pagination.Size).ToList()
            : users;

        return Result<PagedResult<User>>.Success(
            PagedResult<User>.Create(pageItems, users.Count, query.Pagination));
    }
}
