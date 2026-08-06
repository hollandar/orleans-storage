using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public class AlwaysMessagingActiveService : IMessagingActive
{
    public Task<bool> IsMessagingAsync(CancellationToken? ct = null)
    {
        return Task.FromResult(true);
    }
}
