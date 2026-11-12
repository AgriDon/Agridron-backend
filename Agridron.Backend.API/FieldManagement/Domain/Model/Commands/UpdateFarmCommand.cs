namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to update an existing farm.
/// </summary>
public sealed record UpdateFarmCommand(
    long Id,
    string Name,
    string Location,
    long OwnerId,
    string? ImageUrl);
