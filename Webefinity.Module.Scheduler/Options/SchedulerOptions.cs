namespace Webefinity.Module.Scheduler.Options;

public class SchedulerOptions
{
    public int CycleIntervalSeconds { get; set; } = 10;
    public int BackoffMaximumMinutes { get; set; } = 60;
    public double BackoffFactor { get; set; } = 2.0;
}
