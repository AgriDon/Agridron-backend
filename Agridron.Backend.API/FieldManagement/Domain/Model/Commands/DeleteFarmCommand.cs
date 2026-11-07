namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to delete an empty farm.
/// </summary>
public sealed record DeleteFarmCommand(long Id);
