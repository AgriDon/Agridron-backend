namespace Agridron.Backend.API.Shared.Application.Model;

/// <summary>
///     Pagination requested by the client.
/// </summary>
/// <remarks>
///     When neither <c>page</c> nor <c>size</c> is provided the collection is returned unpaged.
///     This keeps compatibility with the frontend built during Sprint 2.
/// </remarks>
public sealed record PaginationParameters
{
    /// <summary>Default page size when pagination is requested without an explicit size.</summary>
    public const int DefaultSize = 20;

    /// <summary>Maximum page size allowed by the API.</summary>
    public const int MaxSize = 100;

    private PaginationParameters(int page, int size, bool isPaged)
    {
        Page = page;
        Size = size;
        IsPaged = isPaged;
    }

    /// <summary>The zero-based page index.</summary>
    public int Page { get; }

    /// <summary>The requested page size.</summary>
    public int Size { get; }

    /// <summary>Whether the client asked for a page.</summary>
    public bool IsPaged { get; }

    /// <summary>Number of records to skip. Zero when the query is unpaged.</summary>
    public int Skip => IsPaged ? Page * Size : 0;

    /// <summary>Represents a query that returns the whole collection.</summary>
    public static PaginationParameters Unpaged { get; } = new(0, 0, false);

    /// <summary>
    ///     Builds validated pagination parameters.
    /// </summary>
    /// <param name="page">Zero-based page index. Negative values are clamped to zero.</param>
    /// <param name="size">Page size. Clamped between 1 and <see cref="MaxSize" />.</param>
    /// <returns>The validated parameters.</returns>
    public static PaginationParameters Create(int? page, int? size)
    {
        if (page is null && size is null)
        {
            return Unpaged;
        }

        var safePage = Math.Max(0, page ?? 0);
        var safeSize = Math.Clamp(size ?? DefaultSize, 1, MaxSize);
        return new PaginationParameters(safePage, safeSize, true);
    }
}
