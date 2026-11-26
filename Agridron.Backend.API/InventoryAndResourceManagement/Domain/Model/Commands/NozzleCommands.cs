using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;

public sealed record CreateNozzleCommand(
    NozzleType Type,
    decimal DiameterMm,
    int StockUnits,
    NozzleStatus Status = NozzleStatus.Good);

public sealed record UpdateNozzleCommand(
    long Id,
    NozzleType Type,
    decimal DiameterMm,
    int StockUnits,
    NozzleStatus Status);

/**
 * the nozzle is deactivated, not removed from the database
 */
public sealed record DeleteNozzleCommand(long Id);

/**
 * Switches the nozzle condition between good and worn
 */
public sealed record ToggleNozzleStatusCommand(long Id);
