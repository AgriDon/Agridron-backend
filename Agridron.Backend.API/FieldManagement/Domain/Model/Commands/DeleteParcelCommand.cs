namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to deactivate or remove a parcel.
/// </summary>
public sealed record DeleteParcelCommand(long Id);
