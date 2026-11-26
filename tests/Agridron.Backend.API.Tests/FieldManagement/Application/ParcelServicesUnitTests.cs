using Agridron.Backend.API.FieldManagement.Application.Internal.CommandServices;
using Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;
using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.Commands;
using Agridron.Backend.API.FieldManagement.Domain.Model.Queries;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Domain.Repositories;
using Agridron.Backend.API.Shared.Application.Model;
using Agridron.Backend.API.Shared.Domain.Repositories;

namespace Agridron.Backend.API.Tests.FieldManagement.Application;

public class ParcelServicesUnitTests
{
    private readonly InMemoryParcelRepository _parcelRepository = new();
    private readonly InMemoryFarmRepository _farmRepository = new();
    private readonly InMemoryCropRepository _cropRepository = new();
    private readonly InMemoryFumigationAreaRepository _fumigationAreaRepository = new();
    private readonly InMemoryUnitOfWork _unitOfWork = new();
    private readonly ParcelCommandService _commandService;
    private readonly ParcelQueryService _queryService;

    public ParcelServicesUnitTests()
    {
        _commandService = new ParcelCommandService(
            _parcelRepository,
            _farmRepository,
            _cropRepository,
            _fumigationAreaRepository,
            _unitOfWork);

        _queryService = new ParcelQueryService(_parcelRepository);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistObstacleNotes()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();

        // Act
        var result = await _commandService.CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06),
            "Flowering",
            null,
            "  High-voltage pole on north corner  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.ObstacleNotes.Should().Be("High-voltage pole on north corner");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndClearObstacleNotes()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var created = await _commandService.CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06),
            obstacleNotes: "Tall eucalyptus trees");

        // Act - update
        var updated = await _commandService.UpdateAsync(
            created.Value!.Id.Value,
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06),
            obstacleNotes: "Irrigation tower at center");

        // Assert - update
        updated.IsSuccess.Should().BeTrue();
        updated.Value!.ObstacleNotes.Should().Be("Irrigation tower at center");

        // Act - clear
        var cleared = await _commandService.UpdateAsync(
            created.Value.Id.Value,
            farm.Id.Value,
            crop.Id.Value,
            "Lote Norte",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06),
            obstacleNotes: "   ");

        // Assert - clear
        cleared.IsSuccess.Should().BeTrue();
        cleared.Value!.ObstacleNotes.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenNoFumigationAreas_ShouldSoftDeleteParcel()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var created = await _commandService.CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Retirable",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06));

        // Act
        var result = await _commandService.DeleteAsync(created.Value!.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        created.Value.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenHasFumigationAreas_ShouldReturnConflict()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var created = await _commandService.CreateAsync(
            farm.Id.Value,
            crop.Id.Value,
            "Lote Con Zonas",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06));
        _fumigationAreaRepository.SetCount(created.Value!.Id, 2);

        // Act
        var result = await _commandService.DeleteAsync(created.Value.Id.Value);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ParcelHasFumigationAreas");
        created.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task QueryList_ShouldRespectIncludeInactiveFlagWithAndWithoutFarmFilter()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var active = (await _commandService.CreateAsync(
            farm.Id.Value, crop.Id.Value, "Lote Activo",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06))).Value!;
        var inactive = (await _commandService.CreateAsync(
            farm.Id.Value, crop.Id.Value, "Lote Inactivo",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06))).Value!;
        await _commandService.DeleteAsync(inactive.Id.Value);

        // Act
        var defaultByFarm = await _queryService.Handle(
            new GetParcelsByFarmIdQuery(farm.Id.Value, PaginationParameters.Unpaged));
        var withInactiveByFarm = await _queryService.Handle(
            new GetParcelsByFarmIdQuery(farm.Id.Value, PaginationParameters.Unpaged, IncludeInactive: true));
        var defaultAll = await _queryService.Handle(
            new GetParcelsByFarmIdQuery(null, PaginationParameters.Unpaged));
        var withInactiveAll = await _queryService.Handle(
            new GetParcelsByFarmIdQuery(null, PaginationParameters.Unpaged, IncludeInactive: true));

        // Assert
        defaultByFarm.Value!.Items.Should().ContainSingle(p => p.Id == active.Id);
        withInactiveByFarm.Value!.Items.Should().HaveCount(2);
        defaultAll.Value!.Items.Should().ContainSingle(p => p.Id == active.Id);
        withInactiveAll.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenInactive_ShouldRequireIncludeInactiveFlag()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var parcel = (await _commandService.CreateAsync(
            farm.Id.Value, crop.Id.Value, "Lote Archivado",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06))).Value!;
        await _commandService.DeleteAsync(parcel.Id.Value);

        // Act
        var hidden = await _queryService.GetByIdAsync(parcel.Id.Value);
        var visible = await _queryService.GetByIdAsync(parcel.Id.Value, includeInactive: true);

        // Assert
        hidden.IsFailure.Should().BeTrue();
        hidden.Error!.Code.Should().Be("ParcelNotFound");
        visible.IsSuccess.Should().BeTrue();
        visible.Value!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateAsync_ShouldReactivateInactiveParcel_AndBeIdempotentWhenActive()
    {
        // Arrange
        var farm = await SeedFarmAsync();
        var crop = await SeedCropAsync();
        var parcel = (await _commandService.CreateAsync(
            farm.Id.Value, crop.Id.Value, "Lote Reactivable",
            TestGeometry.SquareGeoJson(-76.14, -14.07, -76.13, -14.06))).Value!;
        await _commandService.DeleteAsync(parcel.Id.Value);

        // Act
        var activated = await _commandService.ActivateAsync(parcel.Id.Value);
        var commitsAfterActivate = _unitOfWork.CommitsCount;
        var activatedAgain = await _commandService.Handle(new ActivateParcelCommand(parcel.Id.Value));

        // Assert
        activated.IsSuccess.Should().BeTrue();
        activated.Value!.IsActive.Should().BeTrue();
        activatedAgain.IsSuccess.Should().BeTrue();
        _unitOfWork.CommitsCount.Should().Be(commitsAfterActivate);
    }

    [Fact]
    public async Task ActivateAsync_WhenParcelDoesNotExist_ShouldReturnNotFound()
    {
        // Act
        var result = await _commandService.ActivateAsync(9999);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("ParcelNotFound");
    }

    private async Task<Farm> SeedFarmAsync()
    {
        var farm = new Farm("Fundo Demo", "Ica", 1);
        await _farmRepository.AddAsync(farm);
        return farm;
    }

    private async Task<Crop> SeedCropAsync()
    {
        var crop = new Crop("Espárrago", "UC-157");
        await _cropRepository.AddAsync(crop);
        return crop;
    }

    private sealed class InMemoryParcelRepository : IParcelRepository
    {
        private readonly List<Parcel> _items = [];
        private long _nextId = 1;

        public Task AddAsync(Parcel entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Value <= 0)
            {
                TestGeometry.WithId(entity, new ParcelId(_nextId++));
            }

            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Parcel?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(p => p.Id.Value == id));

        public void Update(Parcel entity)
        {
        }

        public void Remove(Parcel entity) => _items.Remove(entity);

        public Task<IEnumerable<Parcel>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Parcel>>(_items.ToList());

        public Task<IReadOnlyList<Parcel>> ListByFarmIdAsync(
            FarmId farmId,
            bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var query = _items.Where(p => p.FarmId == farmId);
            if (onlyActive)
            {
                query = query.Where(p => p.IsActive);
            }

            return Task.FromResult<IReadOnlyList<Parcel>>(query.OrderBy(p => p.Name).ToList());
        }

        public Task<int> CountByFarmIdAsync(
            FarmId farmId,
            bool onlyActive = true,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Count(p => p.FarmId == farmId && (!onlyActive || p.IsActive)));

        public Task<int> CountByCropIdAsync(
            CropId cropId,
            bool onlyActive = true,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Count(p => p.CropId == cropId && (!onlyActive || p.IsActive)));
    }

    private sealed class InMemoryFarmRepository : IFarmRepository
    {
        private readonly List<Farm> _items = [];
        private long _nextId = 1;

        public Task AddAsync(Farm entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Value <= 0)
            {
                TestGeometry.WithId(entity, new FarmId(_nextId++));
            }

            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Farm?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(f => f.Id.Value == id));

        public void Update(Farm entity)
        {
        }

        public void Remove(Farm entity) => _items.Remove(entity);

        public Task<IEnumerable<Farm>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Farm>>(_items.ToList());

        public Task<IReadOnlyList<Farm>> ListByOwnerIdAsync(long ownerId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Farm>>(_items.Where(f => f.OwnerId == ownerId).OrderBy(f => f.Name).ToList());

        public Task<bool> ExistsWithNameAsync(string name, long? excludedId = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Any(f =>
                string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase) &&
                (!excludedId.HasValue || f.Id.Value != excludedId.Value)));
    }

    private sealed class InMemoryCropRepository : ICropRepository
    {
        private readonly List<Crop> _items = [];
        private long _nextId = 1;

        public Task AddAsync(Crop entity, CancellationToken cancellationToken = default)
        {
            if (entity.Id.Value <= 0)
            {
                TestGeometry.WithId(entity, new CropId(_nextId++));
            }

            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task<Crop?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.FirstOrDefault(c => c.Id.Value == id));

        public void Update(Crop entity)
        {
        }

        public void Remove(Crop entity) => _items.Remove(entity);

        public Task<IEnumerable<Crop>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Crop>>(_items.ToList());

        public Task<bool> ExistsWithNameAndVarietyAsync(
            string name,
            string variety,
            long? excludedId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.Any(c =>
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Variety, variety, StringComparison.OrdinalIgnoreCase) &&
                (!excludedId.HasValue || c.Id.Value != excludedId.Value)));
    }

    private sealed class InMemoryFumigationAreaRepository : IFumigationAreaRepository
    {
        private readonly Dictionary<long, int> _countsByParcelId = [];

        public void SetCount(ParcelId parcelId, int count) => _countsByParcelId[parcelId.Value] = count;

        public Task AddAsync(FumigationArea entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<FumigationArea?> FindByIdAsync(long id, CancellationToken cancellationToken = default) =>
            Task.FromResult<FumigationArea?>(null);

        public void Update(FumigationArea entity)
        {
        }

        public void Remove(FumigationArea entity)
        {
        }

        public Task<IEnumerable<FumigationArea>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<FumigationArea>>(Array.Empty<FumigationArea>());

        public Task<IReadOnlyList<FumigationArea>> ListByParcelIdAsync(
            ParcelId parcelId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FumigationArea>>(Array.Empty<FumigationArea>());

        public Task<int> CountByParcelIdAsync(ParcelId parcelId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_countsByParcelId.GetValueOrDefault(parcelId.Value, 0));
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public int CommitsCount { get; private set; }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            CommitsCount++;
            return Task.CompletedTask;
        }
    }
}
