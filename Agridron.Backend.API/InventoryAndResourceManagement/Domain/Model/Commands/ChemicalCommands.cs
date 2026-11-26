using Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.ValueObjects.ValueObjects;

namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Commands;


public sealed record CreateChemicalCommand(
    string Name,
    ChemicalType Type,
    decimal StockLiters,
    decimal MinimumStockLiters,
    DateOnly ExpirationDate);

public sealed record UpdateChemicalCommand(
    long Id,
    string Name,
    ChemicalType Type,
    decimal StockLiters,
    decimal MinimumStockLiters,
    DateOnly ExpirationDate);

/**
 * Soft delete: the chemical is deactivated, not removed from the database
 */
public sealed record DeleteChemicalCommand(long Id);

/**
 * Adds liters to the current stock of a chemical
 */
public sealed record RestockChemicalCommand(long Id, decimal Liters);
