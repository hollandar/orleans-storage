namespace Webefinity.Results;

public class Associated<TValue, TAssociatedValue>
{
    public Associated()
    {
    }

    public Associated(TValue value, TAssociatedValue associatedValue)
    {
        this.Value = value;
        this.AssociatedValue = associatedValue;
    }

    public TValue Value { get; set; } = default!;
    public TAssociatedValue AssociatedValue { get; set; } = default!;

    public static implicit operator (TValue, TAssociatedValue)(Associated<TValue, TAssociatedValue> value) => (value.Value, value.AssociatedValue);
    public static implicit operator Associated<TValue, TAssociatedValue>((TValue value, TAssociatedValue associated) t) => new Associated<TValue, TAssociatedValue>(t.value, t.associated);
}

public class AssociatedResult<TValue, TAssociated> : IResult
{
    public Associated<TValue, TAssociated>? AssociatedValue { get; set; } = null;
    public bool Success { get; set; } = false;
    public string? Message { get; set; } = null;

    public bool HasError => !Success;

    public ResultReasonType Reason { get; set; }

    public AssociatedResult()
    {
    }

    public AssociatedResult(Associated<TValue, TAssociated> value)
    {
        this.Success = true;
        this.Message = null;
        this.AssociatedValue = value;
    }

    public AssociatedResult(string message, ResultReasonType reason = ResultReasonType.None)
    {
        this.Success = false;
        this.Message = message;
        this.Reason = reason;
        this.AssociatedValue = null;
    }

    public static AssociatedResult<TValue, TAssociated> Ok(Associated<TValue, TAssociated> value)
    {
        return new AssociatedResult<TValue, TAssociated>(value) { Success = true };
    }

    public static AssociatedResult<TValue, TAssociated> Fail(string message, ResultReasonType reason = ResultReasonType.None)
    {
        return new AssociatedResult<TValue, TAssociated>(message, reason);
    }

    public static implicit operator (TValue, TAssociated)?(AssociatedResult<TValue, TAssociated> value)
    {
        ArgumentNullException.ThrowIfNull(value?.AssociatedValue, nameof(value));
        if (!value.Success) throw new InvalidOperationException($"Cannot get value from failed result, {value.Message}.");
        return value.AssociatedValue;
    }
    public static implicit operator AssociatedResult<TValue, TAssociated>(Associated<TValue, TAssociated> value) => AssociatedResult<TValue, TAssociated>.Ok(value);
    public static implicit operator Associated<TValue, TAssociated>(AssociatedResult<TValue, TAssociated> value)
    {
        ArgumentNullException.ThrowIfNull(value?.AssociatedValue, nameof(value));
        if (!value.Success) throw new InvalidOperationException($"Cannot get value from failed result, {value.Message}.");
        return value.AssociatedValue;
    }
    public static implicit operator AssociatedResult<TValue, TAssociated>((TValue, TAssociated) value) => AssociatedResult<TValue, TAssociated>.Ok(value);
}
