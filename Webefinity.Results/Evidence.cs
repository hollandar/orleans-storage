namespace Webefinity.Results;

public class Evidence<TEvidenceType>
{
    public Evidence()
    {
        IsSet = false;
        Value = default(TEvidenceType);
    }

    public bool IsSet { get; set; }
    public TEvidenceType? Value { get; set; }

    public void Sign(TEvidenceType value)
    {
        if (IsSet)
        {
            throw new InvalidOperationException("Evidence already signed.");
        }

        IsSet = true;
        Value = value;
    }
}
