namespace Agridron.Backend.API.FieldManagement.Domain.Model.Commands;

/// <summary>
///     Command to reactivate an inactive parcel.
/// </summary>
public sealed record ActivateParcelCommand(long Id);
