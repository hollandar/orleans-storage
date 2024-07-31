namespace Webefinity.Results;
public class ValueResult<TResultType> : IResult
{
    public ValueResult()
    {
        Value = default(TResultType);
    }
    public ValueResult(TResultType value) : base()
    {
        Value = value;
    }

    public bool Success { get; set; }
    public string? Message { get; set; } = null;
    public TResultType? Value { get; set; }
    public bool HasError => Message != null;
    public ResultReasonType Reason { get; set; } = ResultReasonType.None;

    public static ValueResult<TResultType> SuccessResult { get; } = new ValueResult<TResultType>() { Success = true };

    public static ValueResult<TResultType> Ok(TResultType value)
    {
        return new ValueResult<TResultType>(value) { Success = true };
    }

    public static ValueResult<TResultType> Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new ValueResult<TResultType>() { Success = false, Message = message, Reason = reason };
    }

    public static implicit operator ValueResult<TResultType>(bool success)
    {
        return success ? SuccessResult : Fail("Unknown error");
    }

    public static implicit operator ValueResult<TResultType>(string message)
    {
        return Fail(message);
    }

    public static implicit operator TResultType?(ValueResult<TResultType> result)
    {
        if (result.Success == false)
        {
            throw new InvalidOperationException("Cannot get value from failed result");
        }

        return result.Value;
    }

    public static implicit operator ValueResult<TResultType>(TResultType value)
    {
        return new ValueResult<TResultType>(value);
    }
}
