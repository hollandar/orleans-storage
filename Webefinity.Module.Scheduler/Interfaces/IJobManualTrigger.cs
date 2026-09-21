namespace Webefinity.Module.Scheduler.Interfaces;

public interface IJobManualTrigger
{
    void TriggerJob(string jobName);
}
