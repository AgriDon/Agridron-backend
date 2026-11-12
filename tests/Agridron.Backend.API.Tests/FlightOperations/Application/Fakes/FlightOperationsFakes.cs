using Agridron.Backend.API.FieldManagement.Interfaces.ACL;
using Agridron.Backend.API.FlightOperations.Application.Internal.OutboundServices.ACL;
using Agridron.Backend.API.FlightOperations.Domain.Model.Aggregates;
using Agridron.Backend.API.FlightOperations.Domain.Model.ValueObjects;
using Agridron.Backend.API.FlightOperations.Domain.Repositories;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.Tests.FlightOperations.Application.Fakes;

public class FakeUnitOfWork : IUnitOfWork
{
    public int CommitsCount { get; private set; }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        CommitsCount++;
        return Task.CompletedTask;
    }
}

public class FakeDroneRepository : IDroneRepository
{
    private readonly List<Drone> _drones = [];
    private long _idCounter = 1;

    public Task AddAsync(Drone entity, CancellationToken cancellationToken = default)
    {
        // Set private Id via reflection if 0
        if (entity.Id.Value == 0)
        {
            var prop = typeof(Drone).BaseType!.GetProperty("Id")!;
            prop.SetValue(entity, new DroneId(_idCounter++));
        }

        _drones.Add(entity);
        return Task.CompletedTask;
    }

    public Task<Drone?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drones.FirstOrDefault(d => d.Id.Value == id));
    }

    public void Update(Drone entity)
    {
    }

    public void Remove(Drone entity)
    {
        _drones.Remove(entity);
    }

    public Task<IEnumerable<Drone>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Drone>>(_drones);
    }

    public Task<Drone?> FindBySerialNumberAsync(string serialNumber, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drones.FirstOrDefault(d => d.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsWithSerialNumberAsync(string serialNumber, DroneId? excludedId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_drones.Any(d =>
            d.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase) &&
            (!excludedId.HasValue || d.Id.Value != excludedId.Value.Value)));
    }

    public Task<IReadOnlyList<Drone>> ListFilteredAsync(DroneStatus? status, decimal? minCapacity, decimal? maxCapacity, bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _drones.AsEnumerable();
        if (onlyActive) query = query.Where(d => d.IsActive);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);
        if (minCapacity.HasValue) query = query.Where(d => d.CapacityLiters >= minCapacity.Value);
        if (maxCapacity.HasValue) query = query.Where(d => d.CapacityLiters <= maxCapacity.Value);
        return Task.FromResult<IReadOnlyList<Drone>>(query.ToList());
    }
}

public class FakeMissionRepository : IMissionRepository
{
    private readonly List<Mission> _missions = [];
    private long _idCounter = 1;

    public Task AddAsync(Mission entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id.Value == 0)
        {
            var prop = typeof(Mission).BaseType!.GetProperty("Id")!;
            prop.SetValue(entity, new MissionId(_idCounter++));
        }

        _missions.Add(entity);
        return Task.CompletedTask;
    }

    public Task<Mission?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_missions.FirstOrDefault(m => m.Id.Value == id));
    }

    public void Update(Mission entity)
    {
    }

    public void Remove(Mission entity)
    {
        _missions.Remove(entity);
    }

    public Task<IEnumerable<Mission>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Mission>>(_missions);
    }

    public Task<Mission?> FindByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_missions.FirstOrDefault(m => m.Code.Equals(code, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<bool> ExistsWithCodeAsync(string code, MissionId? excludedId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_missions.Any(m =>
            m.Code.Equals(code, StringComparison.OrdinalIgnoreCase) &&
            (!excludedId.HasValue || m.Id.Value != excludedId.Value.Value)));
    }

    public Task<IReadOnlyList<Mission>> ListFilteredAsync(long? farmId, long? parcelId, DroneId? droneId, MissionStatus? status, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var query = _missions.AsEnumerable();
        if (farmId.HasValue) query = query.Where(m => m.FarmId == farmId.Value);
        if (parcelId.HasValue) query = query.Where(m => m.ParcelId == parcelId.Value);
        if (droneId.HasValue) query = query.Where(m => m.DroneId?.Value == droneId.Value.Value);
        if (status.HasValue) query = query.Where(m => m.Status == status.Value);
        if (from.HasValue) query = query.Where(m => m.ScheduledAt >= from.Value);
        if (to.HasValue) query = query.Where(m => m.ScheduledAt <= to.Value);
        return Task.FromResult<IReadOnlyList<Mission>>(query.ToList());
    }
}

public class FakeIncidentRepository : IIncidentRepository
{
    private readonly List<Incident> _incidents = [];
    private long _idCounter = 1;

    public Task AddAsync(Incident entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id.Value == 0)
        {
            var prop = typeof(Incident).BaseType!.GetProperty("Id")!;
            prop.SetValue(entity, new IncidentId(_idCounter++));
        }

        _incidents.Add(entity);
        return Task.CompletedTask;
    }

    public Task<Incident?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_incidents.FirstOrDefault(i => i.Id.Value == id));
    }

    public void Update(Incident entity)
    {
    }

    public void Remove(Incident entity)
    {
        _incidents.Remove(entity);
    }

    public Task<IEnumerable<Incident>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Incident>>(_incidents);
    }

    public Task<IReadOnlyList<Incident>> ListFilteredAsync(MissionId? missionId, DroneId? droneId, string? type, IncidentSeverity? severity, CancellationToken cancellationToken = default)
    {
        var query = _incidents.AsEnumerable();
        if (missionId.HasValue) query = query.Where(i => i.MissionId?.Value == missionId.Value.Value);
        if (droneId.HasValue) query = query.Where(i => i.DroneId?.Value == droneId.Value.Value);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(i => i.Type.Contains(type, StringComparison.OrdinalIgnoreCase));
        if (severity.HasValue) query = query.Where(i => i.Severity == severity.Value);
        return Task.FromResult<IReadOnlyList<Incident>>(query.ToList());
    }
}

public class FakeOperationStatusRepository : IOperationStatusRepository
{
    private readonly List<OperationStatus> _statuses = [];
    private long _idCounter = 1;

    public Task AddAsync(OperationStatus entity, CancellationToken cancellationToken = default)
    {
        if (entity.Id.Value == 0)
        {
            var prop = typeof(OperationStatus).BaseType!.GetProperty("Id")!;
            prop.SetValue(entity, new OperationStatusId(_idCounter++));
        }

        _statuses.Add(entity);
        return Task.CompletedTask;
    }

    public Task<OperationStatus?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_statuses.FirstOrDefault(s => s.Id.Value == id));
    }

    public void Update(OperationStatus entity)
    {
    }

    public void Remove(OperationStatus entity)
    {
        _statuses.Remove(entity);
    }

    public Task<IEnumerable<OperationStatus>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<OperationStatus>>(_statuses);
    }

    public Task<IReadOnlyList<OperationStatus>> ListFilteredAsync(MissionId? missionId, DroneId? droneId, CancellationToken cancellationToken = default)
    {
        var query = _statuses.AsEnumerable();
        if (missionId.HasValue) query = query.Where(s => s.MissionId?.Value == missionId.Value.Value);
        if (droneId.HasValue) query = query.Where(s => s.DroneId?.Value == droneId.Value.Value);
        return Task.FromResult<IReadOnlyList<OperationStatus>>(query.ToList());
    }

    public Task<OperationStatus?> GetLatestByMissionIdAsync(MissionId missionId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_statuses.Where(s => s.MissionId?.Value == missionId.Value)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefault());
    }
}

public class FakeExternalFieldService : IExternalFieldService
{
    public Dictionary<long, ParcelSnapshotDto> Parcels { get; } = new();
    public HashSet<long> ActiveFarms { get; } = [];

    public Task<ParcelSnapshotDto?> FetchParcelSnapshotAsync(long parcelId, CancellationToken cancellationToken = default)
    {
        Parcels.TryGetValue(parcelId, out var snapshot);
        return Task.FromResult(snapshot);
    }

    public Task<bool> ExistsParcelAsync(long parcelId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Parcels.ContainsKey(parcelId));
    }

    public Task<bool> ExistsFarmAsync(long farmId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ActiveFarms.Contains(farmId));
    }
}
