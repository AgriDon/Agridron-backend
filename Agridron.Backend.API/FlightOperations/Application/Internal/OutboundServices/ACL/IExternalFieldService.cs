using Agridron.Backend.API.FieldManagement.Interfaces.ACL;

namespace Agridron.Backend.API.FlightOperations.Application.Internal.OutboundServices.ACL;

/// <summary>
///     Outbound ACL adapter to query field information without direct entity coupling.
/// </summary>
public interface IExternalFieldService
{
    Task<ParcelSnapshotDto?> FetchParcelSnapshotAsync(long parcelId, CancellationToken cancellationToken = default);
    Task<bool> ExistsParcelAsync(long parcelId, CancellationToken cancellationToken = default);
    Task<bool> ExistsFarmAsync(long farmId, CancellationToken cancellationToken = default);
}
