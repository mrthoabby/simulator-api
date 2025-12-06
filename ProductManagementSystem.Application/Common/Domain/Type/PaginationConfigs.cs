using FluentValidation;

namespace ProductManagementSystem.Application.Common.AppEntities.Type;

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
        return new PaginationConfigs(page <= 0 ? 1 : page, pageSize);
    }
}
