using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Webefinity.MessageBus;

public class MessageBus : IDisposable
{
    private SemaphoreSlim semaphoreSlim = new(1);
    private List<Task> unfinishedTasks = new();
    private readonly MessageBusOptions busOptions;
    private readonly ILogger<MessageBus>? logger;
    private readonly IServiceProvider serviceProvider;
    private static int taskCycle = 0;
    private const int taskCleanupInterval = 10;

    public MessageBus(IServiceProvider serviceProvider)
    {
        this.busOptions = serviceProvider.GetRequiredService<MessageBusOptions>();
        this.logger = serviceProvider.GetService<ILogger<MessageBus>>();
        this.serviceProvider = serviceProvider;
    }


    public async Task PublishAsync<TMessageType>(TMessageType message, CancellationToken? ct = null, MessageBusScope? busScope = null)
    {
        if (this.busOptions.TryGetSubscribers<TMessageType>(out var subscriberList))
        {
            int index = 0;
            while (index < subscriberList!.Count)
            {
                var subscriberType = subscriberList[index];
                if (subscriberType.IsAssignableTo(typeof(IMessageSubscriberAsync<TMessageType>)))
                {
                    try
                    {
                        semaphoreSlim.Wait();
                        var task = Task.Run(async () =>
                        {
                            using var scope = serviceProvider.CreateScope();
                            var serviceInstance = scope.ServiceProvider.GetService(subscriberType);

                            var service = serviceInstance as IMessageSubscriberAsync<TMessageType>;

                            try
                            {
                                await service!.HandleAsync(message, ct ?? CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                if (this.busOptions.GetExceptionHandlerType() is not null)
                                {
                                    var exceptionHandler = scope.ServiceProvider.GetRequiredService(this.busOptions.GetExceptionHandlerType()!);
                                    await (exceptionHandler as IMessageBusExceptionHandler)!.HandleExceptionAsync(ex, ct ?? CancellationToken.None);
                                }
                                else if (this.busOptions.GetLogger() && logger is not null)
                                {
                                    logger.LogError(ex, $"MB000: MessageBus / Error handling message {message}.");
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        });

                        AddUnfinishedTask(task);

                        if (busScope is not null && !task.IsCompleted)
                        {
                            busScope.AddTask(task);
                        }
                    }
                    finally { semaphoreSlim.Release(); }
                }

                else if (subscriberType.IsAssignableTo(typeof(IMessageSubscriber<TMessageType>)))
                {
                    using var scope = serviceProvider.CreateScope();
                    var serviceInstance = scope.ServiceProvider.GetService(subscriberType);

                    var service = serviceInstance as IMessageSubscriber<TMessageType>;

                    await service!.HandleAsync(message, ct ?? CancellationToken.None);
                }

                else
                {
                    throw new Exception($"{subscriberType} is not a supported ISubscriber.");
                }
                index++;
            }
        }

    }

    private void AddUnfinishedTask(Task task)
    {
        if (taskCycle++ % taskCleanupInterval == 0)
        {
            unfinishedTasks.RemoveAll(r => r.IsCompleted);
        }

        if (!task.IsCompleted)
        {
            unfinishedTasks.Add(task);
        }
    }

    public bool CompleteTasks(TimeSpan? timeout = null, CancellationToken? ct = null)
    {
        try
        {
            var result = Task.WaitAll(this.unfinishedTasks.ToArray(), (int)(timeout?.TotalMilliseconds ?? 30000), ct ?? CancellationToken.None);
            this.unfinishedTasks.Clear();

            return result;
        }
        finally { semaphoreSlim.Release(); }
    }

    public void Dispose()
    {
        CompleteTasks(TimeSpan.FromSeconds(30));
    }
}
