using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Agridron.Backend.API.Tests.FieldManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Tests.FlightOperations.Infrastructure.Persistence;

[CollectionDefinition(Name)]
public sealed class FlightOperationsDatabaseCollection : ICollectionFixture<FlightOperationsDatabaseFixture>
{
    public const string Name = "flight-operations-database";
}

public sealed class FlightOperationsDatabaseFixture : IAsyncLifetime
{
    private static readonly string[] TruncateStatements =
    [
        "SET FOREIGN_KEY_CHECKS = 0",
        "TRUNCATE TABLE `operation_statuses`",
        "TRUNCATE TABLE `incidents`",
        "TRUNCATE TABLE `missions`",
        "TRUNCATE TABLE `drones`",
        "SET FOREIGN_KEY_CHECKS = 1"
    ];

    public AppDbContext CreateContext()
    {
        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseMySQL(MySqlTestDatabase.ConnectionString)
                .AddInterceptors(new Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors.AuditableEntityInterceptor())
                .Options);
    }

    public async Task InitializeAsync()
    {
        if (!MySqlTestDatabase.IsAvailable)
        {
            return;
        }

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

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

public abstract class FlightOperationsPersistenceTestBase(FlightOperationsDatabaseFixture fixture) : IAsyncLifetime
{
    protected AppDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        Context = fixture.CreateContext();
    }

    public async Task DisposeAsync()
    {
        if (Context != null)
        {
            await Context.DisposeAsync();
        }
    }
}
