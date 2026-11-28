using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to list all registered users with optional pagination.
/// </summary>
/// <param name="Pagination">The pagination parameters applied to the result set.</param>
public sealed record GetAllUsersQuery(PaginationParameters Pagination);
