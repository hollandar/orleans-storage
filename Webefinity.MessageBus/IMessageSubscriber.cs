namespace Webefinity.MessageBus;

public interface IMessageSubscriber;

public interface IMessageSubscriber<TMessageType> : IMessageSubscriber
{
    public Task HandleAsync(TMessageType message, CancellationToken ct);
}

public interface IMessageSubscriberAsync<TMessageType> : IMessageSubscriber
{
    public Task HandleAsync(TMessageType message, CancellationToken ct);
}

