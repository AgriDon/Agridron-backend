using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;

public sealed record GetNozzleByIdQuery(long Id);

/// <summary>Active nozzles, optionally filtered by status.</summary>
public sealed record GetAllNozzlesQuery(NozzleStatus? Status = null);
