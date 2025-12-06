namespace ProductManagementSystem.Application.Common.AppEntities.Type;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public long PageSize { get; set; }
    public long TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    public static PaginatedResult<T> Create(List<T> items, long totalCount, int currentPage, int itemsInPage)
    {
        var totalPages = itemsInPage == 0 ? 1 : (long)Math.Ceiling((double)totalCount / itemsInPage);
        var pageSize = itemsInPage == 0 ? totalCount : itemsInPage;

        return new PaginatedResult<T>
        {
            Items = items ?? [],
            TotalCount = totalCount,
            Page = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = currentPage < totalPages && itemsInPage != 0,
            HasPreviousPage = currentPage > 1 && itemsInPage != 0
        };
    }
}