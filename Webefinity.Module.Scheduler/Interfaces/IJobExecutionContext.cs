namespace Webefinity.Module.Scheduler.Interfaces;

public interface IJobExecutionContext
{
    CancellationToken CancellationToken { get; }
    DateTimeOffset FireTimeUtc { get; }
}
