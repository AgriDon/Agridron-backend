namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

/// <summary>
///     Category of an agrochemical product.
/// </summary>
public enum ChemicalType
{
    /// <summary>Controls weeds.</summary>
    Herbicide = 1,

    /// <summary>Controls insects and pests.</summary>
    Insecticide = 2,

    /// <summary>Controls fungal diseases.</summary>
    Fungicide = 3,

    /// <summary>Provides nutrients to the crop.</summary>
    Fertilizer = 4
}
