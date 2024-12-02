using Webefinity.Module.Messaging.Abstractions.Args;
using Webefinity.Results;

namespace Webefinity.Module.Messaging;

public interface IMessengerService
{
    Task<ValueResult<Guid>> QueueMessageAsync(EmailMessageModel emailMessage);
    Task<ValueResult<Guid>> QueueMessageAsync(SmsMessageModel smsMessageModel);
}
