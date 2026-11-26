using System.Text.Json.Serialization;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Resources;

/**
 * Response of /api/v1/chemicals.
 * Type is "herbicide" | "insecticide" | "fungicide" | "fertilizer" and ExpirationDate is "yyyy-MM-dd".
 */
public sealed record ChemicalResource(
    long Id,
    string Name,
    string Type,
    decimal StockLiters,
    decimal MinimumStockLiters,
    string ExpirationDate,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);

/**
 * Body of POST chemicals
 */
public sealed record CreateChemicalResource(
    string Name,
    string Type,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal StockLiters,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal MinimumStockLiters,
    string? ExpirationDate);

/**
 * Body of PUT chemicals/{id}
 */
public sealed record UpdateChemicalResource(
    string Name,
    string Type,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal StockLiters,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal MinimumStockLiters,
    string? ExpirationDate);


/**
 * Body of POST chemicals/{id}/restock
 */
public sealed record RestockChemicalResource(
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal Liters);
