using Agridron.Backend.API.Shared.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Base repository for all repositories.
/// </summary>
/// <remarks>
///     This class provides basic CRUD operations for all repositories.
///     This class implements the IBaseRepository interface.
/// </remarks>
/// <typeparam name="TEntity">The entity type for the repository.</typeparam>
public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context;

    /// <summary>
    ///     Default constructor for the base repository.
    /// </summary>
    protected BaseRepository(AppDbContext context)
    {
        Context = context;
    }

    /// <inheritdoc />
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var keyProperty = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties[0];
        var keyValue = keyProperty is null || keyProperty.ClrType == typeof(long)
            ? id
            : CreateConvertedKey(keyProperty.ClrType, id);

        return await Context.Set<TEntity>().FindAsync(new object[] { keyValue }, cancellationToken);
    }

    private static object CreateConvertedKey(Type keyType, long id)
    {
        return Activator.CreateInstance(keyType, id)
               ?? throw new InvalidOperationException(
                   $"Unable to create the key type '{keyType.Name}' from the value {id}. " +
                   "Converted keys must expose a constructor that accepts the database identifier.");
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().ToListAsync(cancellationToken);
    }
}
