namespace Webefinity.MessageBus;

public interface ISubscriber;

public interface ISubscriber<TMessageType> : ISubscriber
{
    public Task HandleAsync(TMessageType message, CancellationToken ct);
}

public interface ISubscriberAsync<TMessageType> : ISubscriber
{
    public Task HandleAsync(TMessageType message, CancellationToken ct);
}

