using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context for the Agridron Platform.
/// </summary>
/// <param name="options">The options for the database context.</param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <summary>
    ///     On creating the database model.
    /// </summary>
    /// <param name="builder">The model builder for the database context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Every bounded context declares its mappings as IEntityTypeConfiguration<T>, so the
        // context does not need to be edited when a new bounded context is added.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}
