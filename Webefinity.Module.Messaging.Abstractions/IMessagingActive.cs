namespace Webefinity.Module.Messaging.Abstractions;

public interface IMessagingActive
{
    Task<bool> IsMessagingAsync(CancellationToken? ct = null);
}
