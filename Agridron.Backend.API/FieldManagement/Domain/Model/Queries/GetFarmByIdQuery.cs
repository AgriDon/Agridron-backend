namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to find a farm by its identifier.
/// </summary>
public sealed record GetFarmByIdQuery(long Id);
