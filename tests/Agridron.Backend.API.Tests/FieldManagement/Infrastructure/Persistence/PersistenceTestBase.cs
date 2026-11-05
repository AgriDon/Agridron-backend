using Agridron.Backend.API.FieldManagement.Domain.Model.Aggregates;
using Agridron.Backend.API.FieldManagement.Domain.Model.ValueObjects;
using Agridron.Backend.API.FieldManagement.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;

/// <summary>
///     Groups the persistence tests so they share a single migrated database.
/// </summary>
[CollectionDefinition(Name)]
public sealed class FieldManagementDatabaseCollection : ICollectionFixture<FieldManagementDatabaseFixture>
{
    /// <summary>The name of the collection shared by the persistence tests.</summary>
    public const string Name = "field-management-database";
}

/// <summary>
///     Creates the test database, applies the migrations and resets the field management tables.
/// </summary>
public sealed class FieldManagementDatabaseFixture : IAsyncLifetime
{
    private static readonly string[] TruncateStatements =
    [
        "TRUNCATE TABLE `fumigation_areas`",
        "TRUNCATE TABLE `parcels`",
        "TRUNCATE TABLE `crops`",
        "TRUNCATE TABLE `farms`",
    ];

    /// <summary>Creates a context pointing to the test database.</summary>
    public AppDbContext CreateContext()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseMySQL(MySqlTestDatabase.ConnectionString)
                .Options);
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (!MySqlTestDatabase.IsAvailable)
        {
            return;
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>Removes every row so each test starts from a clean schema.</summary>
    public async Task ResetAsync()
    {
        if (!MySqlTestDatabase.IsAvailable)
        {
            return;
        }

        await using var context = CreateContext();

        foreach (var statement in TruncateStatements)
        {
            await context.Database.ExecuteSqlRawAsync(statement);
        }
    }
}

/// <summary>
///     Base class for the persistence tests. It gives each test a clean database and a context.
/// </summary>
public abstract class PersistenceTestBase : IAsyncLifetime
{
    private readonly FieldManagementDatabaseFixture _fixture;

    /// <summary>
    ///     Initializes the base class.
    /// </summary>
    /// <param name="fixture">The shared database fixture.</param>
    protected PersistenceTestBase(FieldManagementDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>The context used by the test.</summary>
    protected AppDbContext Context { get; private set; } = null!;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
        Context = _fixture.CreateContext();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    /// <summary>Creates a new context pointing to the same test database.</summary>
    protected AppDbContext CreateNewContext()
    {
        return _fixture.CreateContext();
    }

    /// <summary>Reads a row straight from the database to inspect the generated columns.</summary>
    protected async Task<(decimal AreaHectares, int Srid)> ReadComputedGeometryAsync(string table, long id)
    {
        var connection = Context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT area_hectares, ST_SRID(geometry_native) FROM `{table}` WHERE id = @id";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@id";
        parameter.Value = id;
        command.Parameters.Add(parameter);

        await using var reader = await command.ExecuteReaderAsync();

        (await reader.ReadAsync()).Should().BeTrue($"{table} should contain the row with id {id}");

        return (reader.GetDecimal(0), reader.GetInt32(1));
    }

    /// <summary>Persists a farm and returns it with its generated identity.</summary>
    protected async Task<Farm> AddFarmAsync(string name = "La Esperanza", long ownerId = 1)
    {
        var farm = new Farm(name, "Valle del Cauca", ownerId);
        await new FarmRepository(Context).AddAsync(farm);
        await Context.SaveChangesAsync();
        return farm;
    }

    /// <summary>Persists a crop and returns it with its generated identity.</summary>
    protected async Task<Crop> AddCropAsync(string name = "Maiz", string variety = "Amarillo")
    {
        var crop = new Crop(name, variety);
        await new CropRepository(Context).AddAsync(crop);
        await Context.SaveChangesAsync();
        return crop;
    }

    /// <summary>Persists a parcel and returns it with its generated identity.</summary>
    protected async Task<Parcel> AddParcelAsync(
        Farm farm,
        Crop crop,
        string name = "Lote 1",
        GeoJsonPolygon? geometry = null)
    {
        var parcel = new Parcel(
            farm.Id,
            crop.Id,
            name,
            geometry ?? TestGeometry.Square(-78.50, -0.20, -78.49, -0.19));

        await new ParcelRepository(Context).AddAsync(parcel);
        await Context.SaveChangesAsync();
        return parcel;
    }

    /// <summary>Persists a fumigation area and returns it with its generated identity.</summary>
    protected async Task<FumigationArea> AddFumigationAreaAsync(
        Parcel parcel,
        GeoJsonPolygon? geometry = null)
    {
        var area = new FumigationArea(
            parcel.Id,
            geometry ?? TestGeometry.Square(-78.50, -0.20, -78.495, -0.195));

        await new FumigationAreaRepository(Context).AddAsync(area);
        await Context.SaveChangesAsync();
        return area;
    }
}
