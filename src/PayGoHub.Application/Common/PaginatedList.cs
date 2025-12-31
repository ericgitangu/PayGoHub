namespace PayGoHub.Application.Common;

public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        PageSize = pageSize;
        Items = items;
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public int FirstItemIndex => (PageNumber - 1) * PageSize + 1;
    public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalCount);

    public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }

    public IEnumerable<int> GetPageNumbers(int maxPages = 5)
    {
        var startPage = Math.Max(1, PageNumber - maxPages / 2);
        var endPage = Math.Min(TotalPages, startPage + maxPages - 1);

        if (endPage - startPage < maxPages - 1)
        {
            startPage = Math.Max(1, endPage - maxPages + 1);
        }

        for (var i = startPage; i <= endPage; i++)
        {
            yield return i;
        }
    }
}
