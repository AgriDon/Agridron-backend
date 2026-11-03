namespace Agridron.Backend.API.Shared.Application.Model;

/// <summary>
///     A page of results plus the metadata needed by the client to paginate.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public sealed class PagedResult<T>
{
    private PagedResult(IReadOnlyList<T> items, int page, int size, long totalCount)
    {
        Items = items;
        Page = page;
        Size = size;
        TotalCount = totalCount;
    }

    /// <summary>The items of the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>The zero-based page index.</summary>
    public int Page { get; }

    /// <summary>The page size. Zero when the query was unpaged.</summary>
    public int Size { get; }

    /// <summary>Total number of records matching the query.</summary>
    public long TotalCount { get; }

    /// <summary>Total number of pages. One when the query was unpaged.</summary>
    public int TotalPages => Size <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)Size);

    /// <summary>Whether another page exists after this one.</summary>
    public bool HasNextPage => Page + 1 < TotalPages;

    /// <summary>Whether a page exists before this one.</summary>
    public bool HasPreviousPage => Page > 0;

    /// <summary>Creates an empty page.</summary>
    /// <param name="parameters">The pagination parameters used by the query.</param>
    public static PagedResult<T> Empty(PaginationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return new PagedResult<T>([], parameters.Page, parameters.Size, 0);
    }

    /// <summary>
    ///     Creates a page of results.
    /// </summary>
    /// <param name="items">The items of the current page.</param>
    /// <param name="totalCount">Total number of records matching the query.</param>
    /// <param name="parameters">The pagination parameters used by the query.</param>
    /// <returns>The page.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="items" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="totalCount" /> is negative.</exception>
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        long totalCount,
        PaginationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(parameters);
        if (totalCount < 0) throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count cannot be negative.");

        return new PagedResult<T>(items, parameters.Page, parameters.Size, totalCount);
    }

    /// <summary>
    ///     Projects the current page into a page of another type, preserving the metadata.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="selector">The projection.</param>
    /// <returns>A page with the projected items.</returns>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        return new PagedResult<TOut>(Items.Select(selector).ToList(), Page, Size, TotalCount);
    }
}
