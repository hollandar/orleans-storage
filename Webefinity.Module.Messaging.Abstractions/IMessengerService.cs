using Webefinity.Module.Messaging.Abstractions.Models;
using Webefinity.Results;

namespace Webefinity.Module.Messaging.Abstractions;

public interface IMessengerService
{
    Task<ValueResult<Guid>> QueueMessageAsync(string senderId, EmailMessageModel emailMessage, CancellationToken ct = default);
    Task<ValueResult<Guid>> QueueMessageAsync(string senderId, SmsMessageModel smsMessageModel, CancellationToken ct = default);
    Task<PaginatedContainer<MessageListModel>> ListMessagesAsync(PageRequest pageRequest, CancellationToken ct = default);
    Task<ValueResult<MessageModel>> GetMessageAsync(Guid messageId, CancellationToken ct = default);
}
