namespace Webefinity.Module.Messaging.Abstractions;

public class SmsTwilioOptions
{
    public string? AccountSid { get; set; } = string.Empty;
    public string? AuthToken { get; set; } = string.Empty;
    public string? FromNumber { get; set; } = string.Empty;
}

public interface ISmsTwilioOptionsProvider
{
    SmsTwilioOptions GetSmsOptions();
}
