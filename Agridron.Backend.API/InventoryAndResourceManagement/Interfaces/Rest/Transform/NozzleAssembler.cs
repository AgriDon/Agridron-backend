using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;
using Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Resources;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Interfaces.Rest.Transform;

public static class NozzleAssembler
{
    /// <summary>External (JSON) value of a nozzle type</summary>
    public static string ToExternalString(this NozzleType type) => type switch
    {
        NozzleType.HollowCone => "hollow-cone",
        _ => "flat-fan"
    };

    /// <summary>External (JSON) value of a nozzle status</summary>
    public static string ToExternalString(this NozzleStatus status) => status switch
    {
        NozzleStatus.Worn => "worn",
        _ => "good"
    };

    /// <summary>Parses "flat-fan" or "hollow-cone" . It don't matter lower or upper case</summary>
    public static bool TryParseType(string? value, out NozzleType type)
    {
        type = default;
        switch (value?.Trim().ToLowerInvariant())
        {
            case "flat-fan":
                type = NozzleType.FlatFan;
                return true;
            case "hollow-cone":
                type = NozzleType.HollowCone;
                return true;
            default:
                return false;
        }
    }

    /// <summary>Parses "good" or "worn" (case-insensitive). Null or empty means "good" to default.</summary>
    public static bool TryParseStatus(string? value, out NozzleStatus status)
    {
        status = NozzleStatus.Good;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case "good":
                return true;
            case "worn":
                status = NozzleStatus.Worn;
                return true;
            default:
                return false;
        }
    }

    public static NozzleResource ToResourceFrom(Nozzle entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new NozzleResource(
            entity.Id.Value,
            entity.Type.ToExternalString(),
            entity.DiameterMm,
            entity.StockUnits,
            entity.Status.ToExternalString(),
            entity.IsActive,
            entity.CreatedAt,
            entity.UpdatedAt);
    }
}
