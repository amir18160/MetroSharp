using Microsoft.EntityFrameworkCore;

namespace Application.Core;

public class PagedList<T> : List<T>
{
    private int _currentPage;
    private int _pageSize;

    public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        CurrentPage = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)_pageSize);
        AddRange(items);
    }

    public int TotalPages { get; set; }

    public int CurrentPage
    {
        get { return _currentPage; }
        set { _currentPage = (value <= 0) ? 1 : value; }
    }

    public int PageSize
    {
        get { return _pageSize; }
        set { _pageSize = (value <= 0) ? 1 : value; }
    }

    public int TotalCount { get; set; }

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    public PagedList<TU> Map<TU>(Func<T, TU> mapFunc)
    {
        var mappedItems = this.Select(mapFunc).ToList();
        return new PagedList<TU>(mappedItems, TotalCount, CurrentPage, PageSize);
    }
}