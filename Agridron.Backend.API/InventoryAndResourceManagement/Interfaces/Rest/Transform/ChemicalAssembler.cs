using System.Globalization;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Transform;

/// <summary>
///     Maps chemicals between the domain and the REST layer.
/// </summary>
public static class ChemicalAssembler
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>External (JSON) value of a chemical type, the same used by  the enum types.</summary>
    public static string ToExternalString(this ChemicalType type) => type.ToString().ToLowerInvariant();

    /// <summary>Parses "herbicide", "insecticide", "fungicide" or "fertilizer" (case-insensitive).</summary>
    public static bool TryParseType(string? value, out ChemicalType type)
    {
        type = default;
        return !string.IsNullOrWhiteSpace(value)
               && Enum.TryParse(value.Trim(), true, out type)
               && Enum.IsDefined(type);
    }

    /// <summary>Parses "yyyy-MM-dd"</summary>
    public static bool TryParseExpirationDate(string? value, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        if (DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return true;
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
        {
            date = DateOnly.FromDateTime(dateTime.UtcDateTime);
            return true;
        }

        return false;
    }

    public static ChemicalResource ToResourceFrom(Chemical entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ChemicalResource(
            entity.Id.Value,
            entity.Name,
            entity.Type.ToExternalString(),
            entity.StockLiters,
            entity.MinimumStockLiters,
            entity.ExpirationDate.ToString(DateFormat, CultureInfo.InvariantCulture),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
