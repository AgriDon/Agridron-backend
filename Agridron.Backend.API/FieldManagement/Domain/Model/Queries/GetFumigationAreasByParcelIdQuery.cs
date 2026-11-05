using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to list fumigation areas optionally filtered by parcel and paginated.
/// </summary>
public sealed record GetFumigationAreasByParcelIdQuery(long? ParcelId, PaginationParameters Pagination);
