namespace ProductManagementSystem.Application.Common.Domain.Type;

public class PaginationConfigs
{
    public int Page { get; set; }
    public int PageSize { get; set; }

    private PaginationConfigs(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public static PaginationConfigs Create(int page, int pageSize)
    {
        return new PaginationConfigs(page, pageSize);
    }
}