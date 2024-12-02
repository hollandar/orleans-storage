namespace Webefinity.Module.Messaging.Abstractions.Args;

public class SmsMessageModel
{
    public string[] To { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}
