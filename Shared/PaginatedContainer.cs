namespace Shared;

public class PaginatedContainer<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; } = 0;
}

public class PageRequest
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? Search { get; set; } = null;
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public OrderBy OrderBy { get; set; } = OrderBy.Id;
}
