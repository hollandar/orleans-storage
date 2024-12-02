using Webefinity.Module.Messaging.Abstractions.Args;

namespace Webefinity.Module.Messaging.Abstractions;

public interface ISmsSender
{
    Task SendAsync(SmsMessageModel smsMessage, CancellationToken ct);
}
