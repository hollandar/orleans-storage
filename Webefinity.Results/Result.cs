namespace Webefinity.Results;


public class Result : IResult
{
    public Result()
    {
        Success = true;
        Message = null;
        Reason = ResultReasonType.None;
    }

    public Result(string message, ResultReasonType reason)
    {
        Success = false;
        Message = message;
        Reason = reason;
    }

    public bool Success { get; set; }
    public string? Message { get; set; } = null;
    public bool HasError => Message != null;
    public ResultReasonType Reason { get; set; } = ResultReasonType.None;

    public static Result SuccessResult { get; } = new Result() { Success = true };


    public static Result Ok()
    {
        return SuccessResult;
    }

    public static Result Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new Result() { Success = false, Message = message, Reason = reason };
    }

    public static implicit operator Result(bool success)
    {
        return success ? SuccessResult : Fail("Unknown error");
    }

    public static implicit operator Result(string message)
    {
        return Fail(message);
    }

    public static implicit operator bool(Result result) => result.Success;
    public static Result operator &(Result result1, Result result2)
    {
        if (result1.Success && result2.Success)
        {
            return SuccessResult;
        }
        else
        {
            return Result.Fail(result1.Message + " " + result2.Message);
        }
    }
    
    public static Result operator |(Result result1, Result result2)
    {
        if (result1.Success || result2.Success)
        {
            return SuccessResult;
        }
        else
        {
            return Result.Fail(result1.Message + " " + result2.Message);
        }
    }
}
