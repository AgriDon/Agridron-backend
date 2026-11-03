namespace Agridron.Backend.API.Shared.Domain.Model.Entities;

/// <summary>
///     Base class for every domain entity.
/// </summary>
/// <remarks>
///     Two entities are the same when they are of the same type and share the same identity,
///     regardless of the value of their other attributes.
/// </remarks>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    ///     Creates an entity with the given identity.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    ///     Parameterless constructor required by Entity Framework Core materialization.
    /// </summary>
    protected Entity()
    {
    }

    /// <summary>
    ///     The entity identifier.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType()
               && EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> entity && Equals(entity);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return object.Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !object.Equals(left, right);
    }
}
