using System.Diagnostics;

namespace Webefinity.Results;
public class ValueResult<TResultType> : IResult
{
    public ValueResult()
    {
        Value = default(TResultType);
    }

    private ValueResult(TResultType value) : base()
    {
        Value = value;
    }

    public bool Success { get; set; }
    public string Message => String.Join(',', Messages);
    public string[] Messages { get; set; } = [];
    public TResultType? Value { get; set; }
    public bool HasError => Message != null;
    public ResultReasonType Reason { get; set; } = ResultReasonType.None;

    public static ValueResult<TResultType> SuccessResult { get; } = new ValueResult<TResultType>() { Success = true };

    public static ValueResult<TResultType> Ok(TResultType value)
    {
        //Debug.Assert(value is not null);
        return new ValueResult<TResultType>(value) { Success = true };
    }

    public static ValueResult<TResultType> Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new ValueResult<TResultType>() { Success = false, Messages = [message], Reason = reason };
    }

    public static ValueResult<TResultType> Fail(string[] messages, ResultReasonType reason = ResultReasonType.None)
    {
        return new ValueResult<TResultType>() { Success = false, Messages = messages, Reason = reason };
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

    public static implicit operator ValueResult<TResultType>(Result result)
    {
        return new ValueResult<TResultType>() { Success = result.Success, Messages = result.Messages, Reason = result.Reason };
    }

    public static implicit operator Result(ValueResult<TResultType> result)
    {
        return new Result() { Success = result.Success, Messages = result.Messages, Reason = result.Reason };
    }
}
