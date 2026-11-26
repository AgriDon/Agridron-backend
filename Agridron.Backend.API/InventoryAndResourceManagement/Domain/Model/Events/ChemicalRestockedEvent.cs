
using Agridron.Backend.API.Shared.Domain.Model.Events;
namespace Agridron.Backend.API.InventoryAndResourceManagement.Domain.Model.Events;

/// <summary>
///     Raised when liters are added to the stock of a chemical litters
/// </summary>
/// <param name="ChemicalId">The restocked chemical.</param>
/// <param name="AddedLiters">Liters added in this restock.</param>
/// <param name="NewStockLiters">Stock after the restock/recharge.</param>
/// <param name="OccurredAt">When the restock happened.</param>
public sealed record ChemicalRestockedEvent(
    long ChemicalId,
    decimal AddedLiters,
    decimal NewStockLiters,
    DateTimeOffset OccurredAt) : IEvent;
