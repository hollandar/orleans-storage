namespace Webefinity.Results;

public class ListResult<TItemType> : IResult
{
    public ListResult()
    {

    }

    public ListResult(List<TItemType> items)
    {
        this.Items = items;
    }

    public static ListResult<TItemType> Ok(List<TItemType> items)
    {
        return new ListResult<TItemType>(items) { Success = true };
    }

    public static ListResult<TItemType> Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new ListResult<TItemType>() { Success = false, Message = message, Reason = reason };
    }



    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<TItemType> Items { get; set; } = new();

    public bool HasError => Success == false;

    public ResultReasonType Reason { get; set; }
}
