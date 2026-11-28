using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Application.QueryServices;

/// <summary>
///     Application query service contract for reading IAM user data.
/// </summary>
public interface IUserQueryService
{
    /// <summary>
    ///     Finds a user by its database identifier.
    /// </summary>
    /// <param name="query">The identifier lookup query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the matching <see cref="User" /> or a not-found error.</returns>
    Task<Result<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a user by its email address.
    /// </summary>
    /// <param name="query">The email lookup query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing the matching <see cref="User" /> or an error.</returns>
    Task<Result<User>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists registered users with optional pagination.
    /// </summary>
    /// <param name="query">The pagination query.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Result{T}" /> containing a <see cref="PagedResult{T}" /> of users.</returns>
    Task<Result<PagedResult<User>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default);
}
