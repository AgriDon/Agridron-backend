namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

/// <summary>
/// two types of nozzles
/// </summary>
public enum NozzleType
{
    /// <summary>Flat fan pattern ("flat-fan"): uniform band, used for herbicides.</summary>
    FlatFan = 1,

    /// <summary>Hollow cone pattern ("hollow-cone"): fine droplets, used for insecticides and fungicides.</summary>
    HollowCone = 2
}
