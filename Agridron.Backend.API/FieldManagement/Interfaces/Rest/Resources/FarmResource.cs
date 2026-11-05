namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     A farm as returned by the API.
/// </summary>
public sealed record FarmResource(
    long Id,
    string Name,
    string Location,
    long OwnerId,
    string? ImageUrl);
