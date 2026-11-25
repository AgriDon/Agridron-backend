namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

/// <summary>
/// Only two status of nozzles
/// </summary>
public enum NozzleStatus
{
    /// <summary>Ready to be used ("good").</summary>
    Good = 1,

    /// <summary>Worn out, MUST be replaced ("worn").</summary>
    Worn = 2
}
