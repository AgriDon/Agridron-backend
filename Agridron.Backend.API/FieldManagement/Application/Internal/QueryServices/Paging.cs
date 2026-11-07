using Agridron.Backend.API.Shared.Application.Model;

namespace Agridron.Backend.API.FieldManagement.Application.Internal.QueryServices;

/// <summary>
///     Applies the requested pagination to an in-memory collection.
/// </summary>
internal static class Paging
{
    /// <summary>
    ///     Returns the requested page while preserving the total count.
    /// </summary>
    public static PagedResult<T> Apply<T>(IReadOnlyList<T> items, PaginationParameters pagination)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(pagination);

        var pageItems = pagination.IsPaged
            ? items.Skip(pagination.Skip).Take(pagination.Size).ToList()
            : items;

        return PagedResult<T>.Create(pageItems, items.Count, pagination);
    }
}
