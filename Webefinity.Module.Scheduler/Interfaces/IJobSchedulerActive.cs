namespace Webefinity.Module.Scheduler.Interfaces;

public interface IJobSchedulerActive
{
    Task<bool> IsJobSchedulerActiveAsync(CancellationToken? cancellationToken = null);
}
