using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to list farms optionally filtered by owner and paginated.
/// </summary>
public sealed record GetFarmsByOwnerIdQuery(long? OwnerId, PaginationParameters Pagination);
