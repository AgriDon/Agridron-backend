
using Agridron.Backend.API.Shared.Domain.Model;
namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Errors;

/// <summary>
///     Error catalog of the Inventory &amp; Resource Management bounded context.
/// </summary>
public static class InventoryErrors
{


    public static readonly Error ChemicalNameRequired =
        Error.Validation("ChemicalNameRequired", "The chemical name is required.");

    public static readonly Error ChemicalNameTooLong =
        Error.Validation("ChemicalNameTooLong", "The chemical name cannot exceed 100 characters.");

    public static readonly Error ChemicalTypeInvalid =
        Error.Validation("ChemicalTypeInvalid", "The chemical type must be 'herbicide', 'insecticide', 'fungicide' or 'fertilizer'.");

    public static readonly Error ChemicalStockInvalid =
        Error.Validation("ChemicalStockInvalid", "The stock in liters must be greater than or equal to zero.");

    public static readonly Error ChemicalMinimumStockInvalid =
        Error.Validation("ChemicalMinimumStockInvalid", "The minimum stock in liters must be greater than or equal to zero.");

    public static readonly Error ChemicalExpirationDateRequired =
        Error.Validation("ChemicalExpirationDateRequired", "The expiration date is required and must use the format yyyy-MM-dd.");

    public static readonly Error ChemicalRestockAmountInvalid =
        Error.Validation("ChemicalRestockAmountInvalid", "The restock amount must be greater than zero liters.");

    public static Error ChemicalNotFound(long id) =>
        Error.NotFound("ChemicalNotFound", $"Chemical with id {id} was not found.");


    // ---------------------------------------------------------------------
    // Nozzle Errors
    // ---------------------------------------------------------------------

    public static readonly Error NozzleTypeInvalid =
        Error.Validation("NozzleTypeInvalid", "The nozzle type must be 'flat-fan' or 'hollow-cone'.");

    public static readonly Error NozzleDiameterInvalid =
        Error.Validation("NozzleDiameterInvalid", "The nozzle diameter in millimeters must be greater than zero.");

    public static readonly Error NozzleStockInvalid =
        Error.Validation("NozzleStockInvalid", "The nozzle stock units must be greater than or equal to zero.");

    public static readonly Error NozzleStatusInvalid =
        Error.Validation("NozzleStatusInvalid", "The nozzle status must be 'good' or 'worn'.");

    public static Error NozzleNotFound(long id) =>
        Error.NotFound("NozzleNotFound", $"Nozzle with id {id} was not found.");



}
