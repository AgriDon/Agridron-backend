namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to register a new farm.
/// </summary>
public sealed record CreateFarmCommand(
    string Name,
    string Location,
    long OwnerId,
    string? ImageUrl);
