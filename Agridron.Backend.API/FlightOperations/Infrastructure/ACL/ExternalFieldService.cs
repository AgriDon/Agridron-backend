using Agridron.Backend.API.FieldManagement.Interfaces.ACL;
using Agridron.Backend.API.FlightOperations.Application.Internal.OutboundServices.ACL;

namespace Agridron.Backend.API.FlightOperations.Infrastructure.ACL;

/// <summary>
///     Concrete implementation of <see cref="IExternalFieldService" /> delegating to
///     <see cref="IFieldManagementContextFacade" />.
/// </summary>
public sealed class ExternalFieldService(IFieldManagementContextFacade fieldManagementContextFacade) : IExternalFieldService
{
    public Task<ParcelSnapshotDto?> FetchParcelSnapshotAsync(long parcelId, CancellationToken cancellationToken = default) =>
        fieldManagementContextFacade.FetchParcelByIdAsync(parcelId, cancellationToken);

    public Task<bool> ExistsParcelAsync(long parcelId, CancellationToken cancellationToken = default) =>
        fieldManagementContextFacade.ExistsActiveParcelAsync(parcelId, cancellationToken);

    public Task<bool> ExistsFarmAsync(long farmId, CancellationToken cancellationToken = default) =>
        fieldManagementContextFacade.ExistsActiveFarmAsync(farmId, cancellationToken);
}
