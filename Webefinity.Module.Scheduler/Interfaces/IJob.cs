namespace Webefinity.Module.Scheduler.Interfaces;

public interface IJob
{
    Task ExecuteAsync(IJobExecutionContext context);
}
