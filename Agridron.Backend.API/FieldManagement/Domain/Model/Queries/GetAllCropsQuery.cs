using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to list all crops with optional pagination.
/// </summary>
public sealed record GetAllCropsQuery(PaginationParameters Pagination);
