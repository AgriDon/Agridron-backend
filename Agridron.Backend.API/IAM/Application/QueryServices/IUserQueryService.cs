using Agridron.Backend.API.IAM.Domain.Model.Aggregates;
using Agridron.Backend.API.IAM.Domain.Model.Queries;
using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Application.QueryServices;

/// <summary>Application service contract for read-only user queries.</summary>
public interface IUserQueryService
{
    /// <summary>Finds a user by database identifier.</summary>
    Task<Result<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by email address.</summary>
    Task<Result<User>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken = default);

    /// <summary>Lists users with optional pagination.</summary>
    Task<Result<PagedResult<User>>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken = default);
}
