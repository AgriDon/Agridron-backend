using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to list parcels optionally filtered by farm and paginated.
/// </summary>
public sealed record GetParcelsByFarmIdQuery(long? FarmId, PaginationParameters Pagination);
