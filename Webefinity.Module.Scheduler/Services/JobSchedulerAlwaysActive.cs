using Webefinity.Module.Scheduler.Interfaces;

namespace Webefinity.Module.Scheduler.Services;

public class JobSchedulerAlwaysActive : IJobSchedulerActive
{
    public Task<bool> IsJobSchedulerActiveAsync(CancellationToken? cancellationToken = null)
    {
        return Task.FromResult(true);
    }
}
