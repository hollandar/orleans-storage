namespace Shared;

public interface IResult
{
    bool Success { get; set; }
    public string? Message { get; set; }
    public bool HasError { get; }
    public ResultReasonType Reason { get; }
}

