namespace CrisisPlatform.Api.Common.Pagination;

/// <summary>Normalised pagination parameters with safe bounds.</summary>
public sealed record PaginationQuery
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize     = 100;

    public int Page     { get; }
    public int PageSize { get; }

    public PaginationQuery(int page = 1, int pageSize = DefaultPageSize)
    {
        Page     = page < 1 ? 1 : page;
        PageSize = pageSize switch
        {
            < 1           => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _             => pageSize,
        };
    }

    public int Skip => (Page - 1) * PageSize;
}

/// <summary>Standard paginated envelope returned by list endpoints.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int              Page,
    int              PageSize,
    int              TotalCount)
{
    public int  TotalPages  => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext     => Page < TotalPages;
    public bool HasPrevious => Page > 1 && TotalPages > 0;
}

/// <summary>Applies <see cref="PaginationQuery"/> to an <see cref="IEnumerable{T}"/>.</summary>
public static class PaginationExtensions
{
    public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> source, PaginationQuery query)
    {
        var list = source as IReadOnlyCollection<T> ?? source.ToList();
        var items = list.Skip(query.Skip).Take(query.PageSize).ToList();
        return new PagedResult<T>(items, query.Page, query.PageSize, list.Count);
    }
}
