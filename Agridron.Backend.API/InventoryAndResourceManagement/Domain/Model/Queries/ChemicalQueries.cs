using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Queries;

public sealed record GetChemicalByIdQuery(long Id);

/// <summary>Active chemicals, optionally filtered by type.</summary>
public sealed record GetAllChemicalsQuery(ChemicalType? Type = null);
