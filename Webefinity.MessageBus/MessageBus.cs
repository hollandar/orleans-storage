using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Webefinity.MessageBus;

public class MessageBus : IDisposable
{
    private SemaphoreSlim semaphoreSlim = new(1);
    private List<Task> unfinishedTasks = new();
    private readonly BusOptions busOptions;
    private readonly ILogger<MessageBus>? logger;
    private readonly IServiceProvider serviceProvider;
    private static int taskCycle = 0;
    private const int taskCleanupInterval = 10;

    public MessageBus(IServiceProvider serviceProvider)
    {
        this.busOptions = serviceProvider.GetRequiredService<BusOptions>();
        this.logger = serviceProvider.GetService<ILogger<MessageBus>>();
        this.serviceProvider = serviceProvider;
    }


    public async Task PublishAsync<TMessageType>(TMessageType message, CancellationToken? ct = null, BusScope? busScope = null)
    {
        using var scope = serviceProvider.CreateScope();
        if (this.busOptions.TryGetSubscribers<TMessageType>(out var subscriberList))
        {
            int index = 0;
            while (index < subscriberList!.Count)
            {
                var subscriberType = subscriberList[index];
                var serviceInstance = scope.ServiceProvider.GetService(subscriberType);
                if (serviceInstance is ISubscriberAsync<TMessageType>)
                {
                    try
                    {
                        semaphoreSlim.Wait();

                        var service = serviceInstance as ISubscriberAsync<TMessageType>;
                        var task = Task.Run(async () =>
                        {
                            try
                            {
                                await service!.HandleAsync(message, ct ?? CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                if (this.busOptions.GetExceptionHandlerType() is not null)
                                {
                                    var exceptionHandler = scope.ServiceProvider.GetRequiredService(this.busOptions.GetExceptionHandlerType()!);
                                    await (exceptionHandler as IBusExceptionHandler)!.HandleExceptionAsync(ex, ct ?? CancellationToken.None);
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

                else if (serviceInstance is ISubscriber<TMessageType>)
                {
                    var service = serviceInstance as ISubscriber<TMessageType>;

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
