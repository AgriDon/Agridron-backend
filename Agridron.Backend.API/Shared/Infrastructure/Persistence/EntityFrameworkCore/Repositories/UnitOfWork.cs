using Agridron.Backend.API.Shared.Domain.Repositories;
using Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Agridron.Backend.API.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Unit of work implementation for coordinating database transactions.
/// </summary>
/// <remarks>
///     This class implements the IUnitOfWork contract and encapsulates EF Core's
///     SaveChangesAsync method.
/// </remarks>
/// <param name="context">The EF Core database context.</param>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
