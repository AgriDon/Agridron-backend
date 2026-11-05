namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to find a fumigation area by its identifier.
/// </summary>
public sealed record GetFumigationAreaByIdQuery(long Id);
