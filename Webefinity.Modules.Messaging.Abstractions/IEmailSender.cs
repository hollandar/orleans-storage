using Webefinity.Module.Messaging.Abstractions.Models;

namespace Webefinity.Module.Messaging.Abstractions;

public interface IEmailSender
{
    Task SendAsync(EmailMessageModel emailMessage, CancellationToken? ct);
}
