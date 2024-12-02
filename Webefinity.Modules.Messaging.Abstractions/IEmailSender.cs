using Webefinity.Module.Messaging.Abstractions.Args;

namespace Webefinity.Module.Messaging.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessageModel emailMessage, CancellationToken? ct);
}
