using Webefinity.Module.Scheduler.Interfaces;

namespace Webefinity.Module.Scheduler.Services.Context;

internal class JobExecutionContext : IJobExecutionContext
{
    public string JobName { get; init; } = string.Empty;
    public DateTimeOffset ScheduledTime { get; init; }
    public DateTimeOffset FireTimeUtc { get; init; }
    public DateTimeOffset? LastRunTime { get; init; }
    public TimeSpan? Interval { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
