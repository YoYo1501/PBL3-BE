namespace BackendAPI.Models.DTOs.Common;

public class PagedQueryDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public int GetPage() => Page < 1 ? 1 : Page;

    public int GetPageSize(int defaultPageSize = 10, int maxPageSize = 50)
    {
        if (PageSize <= 0) return defaultPageSize;
        return PageSize > maxPageSize ? maxPageSize : PageSize;
    }
}

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
