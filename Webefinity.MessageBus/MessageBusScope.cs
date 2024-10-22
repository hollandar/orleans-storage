namespace Webefinity.MessageBus;


public class MessageBusScope : IDisposable
{
    private static int taskCycle = 0;
    private const int taskCleanupInterval = 10;
    private readonly List<Task> unfinishedTasks = new List<Task>();

    public void AddTask(Task task) {
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
        var result = Task.WaitAll(unfinishedTasks.ToArray(), (int)(timeout?.TotalMilliseconds ?? 30000), ct ?? CancellationToken.None);
        unfinishedTasks.Clear();

        return result;
    }

    public void Dispose()
    {
        CompleteTasks(TimeSpan.FromSeconds(30));
    }
}
