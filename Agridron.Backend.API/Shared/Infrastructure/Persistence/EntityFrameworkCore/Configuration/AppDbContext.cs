using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context for the Agridron Platform.
/// </summary>
/// <param name="options">The options for the database context.</param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Apply audit timestamp interceptor for all IAuditableEntity implementations
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <summary>
    ///     On creating the database model.
    /// </summary>
    /// <param name="builder">The model builder for the database context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Bounded context entity configurations will be applied here via extensions

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}
