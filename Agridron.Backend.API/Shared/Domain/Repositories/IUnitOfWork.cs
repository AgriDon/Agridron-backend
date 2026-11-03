namespace Agridron.Backend.API.Shared.Domain.Repositories;

/// <summary>
///     Unit of work interface for coordinating database transactions.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Save changes to the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
