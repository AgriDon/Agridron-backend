using System.Text.Json.Serialization;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Resources;

/// <summary>
/// Response of /api/v1/nozzles.
/// </summary>
/// <param name="Id"></param>
/// <param name="Type"></param>
/// <param name="DiameterMm"></param>
/// <param name="StockUnits"></param>
/// <param name="Status"></param>
/// <param name="IsActive"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
public sealed record NozzleResource(
    long Id,
    string Type,
    decimal DiameterMm,
    int StockUnits,
    string Status,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);


/// <summary>
/// Body of POST /api/v1/nozzles (status is optional, "good" by default)
/// </summary>
/// <param name="Type"></param>
/// <param name="DiameterMm"></param>
/// <param name="StockUnits"></param>
/// <param name="Status"></param>
public sealed record CreateNozzleResource(
    string Type,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal DiameterMm,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int StockUnits,
    string? Status);

/// <summary>
///  Body of PUT /api/v1/nozzles/{id}
/// </summary>
/// <param name="Type"></param>
/// <param name="DiameterMm"></param>
/// <param name="StockUnits"></param>
/// <param name="Status"></param>
public sealed record UpdateNozzleResource(
    string Type,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] decimal DiameterMm,
    [property: JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] int StockUnits,
    string? Status);
