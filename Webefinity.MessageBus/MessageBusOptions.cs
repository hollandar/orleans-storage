﻿using System.Collections.Concurrent;

namespace Webefinity.MessageBus;

public interface IMessageBusOptions
{
    void Subscribe<TSubscriberType, TMessageType>() where TSubscriberType : IMessageSubscriber;
}

public interface IMessageBusExceptionHandler
{
    Task HandleExceptionAsync(Exception exception, CancellationToken ct);
}

public class MessageBusOptions : IMessageBusOptions
{
    private Type? exceptionHandler = null;
    private bool useLogger = true;
    private ConcurrentDictionary<Type, List<Type>> subscribers = new();

    public void Subscribe<TSubscriberType, TMessageType>() where TSubscriberType : IMessageSubscriber
    {
        if (!subscribers.ContainsKey(typeof(TMessageType)))
        {
            subscribers[typeof(TMessageType)] = new();
        }
        subscribers[typeof(TMessageType)].Add(typeof(TSubscriberType));
    }

    public bool TryGetSubscribers<TMessageType>(out List<Type>? subscribers)
    {
        return this.subscribers.TryGetValue(typeof(TMessageType), out subscribers);
    }

    public void UseExceptionHandler<TBusExceptionHandler>() where TBusExceptionHandler : IMessageBusExceptionHandler
    {
        this.exceptionHandler = typeof(TBusExceptionHandler);
    }

    public void UseLogger(bool useLogger = true)
    {
        this.useLogger = useLogger;
    }

    public HashSet<Type> GetSubscriberTypes() => subscribers.Values.SelectMany(r => r).ToHashSet();
    public Type? GetExceptionHandlerType() => this.exceptionHandler;
    public bool GetLogger() => this.useLogger;
}
