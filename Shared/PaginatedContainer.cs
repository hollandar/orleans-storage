namespace Shared;

public class PaginatedContainer<T>
{
    public PaginatedContainer() { }
    public PaginatedContainer(IEnumerable<T> items, int totalCount)
    {
        Items = items.ToArray();
        TotalCount = totalCount;
        Reason = ResultReasonType.None;
        Message = null;
    }
    public PaginatedContainer(string message, ResultReasonType reasonType)
    {
        this.Message = message;
        this.Reason = reasonType;
        this.Items = Array.Empty<T>();
        this.TotalCount = 0;
    }

    public ICollection<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; } = 0;
    public bool Success => (Message is null && Reason == ResultReasonType.None);
    public string? Message { get; set; } = null;
    public ResultReasonType Reason { get; set; } = ResultReasonType.None;

    public static PaginatedContainer<T> Fail(string message, ResultReasonType reason = ResultReasonType.None) {
        return new PaginatedContainer<T>(message, reason);
    }
}

public class PageRequest
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
    public string? Search { get; set; } = null;
    public SortOrder SortOrder { get; set; } = SortOrder.Asc;
    public OrderBy OrderBy { get; set; } = OrderBy.Id;
}

public class PageRequestWithId:PageRequest
{
    public string Id { get; set; } = string.Empty;
}
