namespace Agridron.Backend.API.FieldManagement.Interfaces.Rest.Resources;

/// <summary>
///     A crop as returned by the API.
/// </summary>
public sealed record CropResource(long Id, string Name, string Variety);
