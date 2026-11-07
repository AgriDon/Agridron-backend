namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to delete an unassigned crop.
/// </summary>
public sealed record DeleteCropCommand(long Id);
