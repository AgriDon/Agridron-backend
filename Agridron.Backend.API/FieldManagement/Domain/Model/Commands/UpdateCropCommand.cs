namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to update an existing crop.
/// </summary>
public sealed record UpdateCropCommand(long Id, string Name, string Variety);
