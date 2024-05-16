namespace Shared;

public class PaginatedContainer<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; } = 0;
}
