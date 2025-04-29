namespace Webefinity.Results;

public class ListResult<TItemType> : IResult
{
    public ListResult()
    {

    }

    public ListResult(ICollection<TItemType> items)
    {
        this.Items = items;
    }

    public static ListResult<TItemType> Ok(ICollection<TItemType> items)
    {
        return new ListResult<TItemType>(items) { Success = true };
    }

    public static ListResult<TItemType> Empty() => Ok(Array.Empty<TItemType>());

    public static ListResult<TItemType> Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new ListResult<TItemType>() { Success = false, Message = message, Reason = reason };
    }



    public bool Success { get; set; }
    public string? Message { get; set; }
    public ICollection<TItemType> Items { get; set; } = Array.Empty<TItemType>();

    public bool HasError => Success == false;

    public ResultReasonType Reason { get; set; }
}
