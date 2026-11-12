namespace Agridron.Backend.API.FieldManagement.Domain.Model.Queries;

/// <summary>
///     Query to find a parcel by its identifier.
/// </summary>
public sealed record GetParcelByIdQuery(long Id);
