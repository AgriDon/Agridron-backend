namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to register a crop.
/// </summary>
public sealed record CreateCropCommand(string Name, string Variety);
