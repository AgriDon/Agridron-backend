namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to find a crop by its identifier.
/// </summary>
public sealed record GetCropByIdQuery(long Id);
