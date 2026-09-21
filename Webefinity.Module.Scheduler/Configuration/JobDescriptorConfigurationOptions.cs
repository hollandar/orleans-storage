using Webefinity.Module.Scheduler.Conditions;
using Webefinity.Module.Scheduler.Interfaces;

namespace Webefinity.Module.Scheduler.Configuration;

public class JobDescriptorConfigurationOptions
{
    private readonly List<JobDescriptor> jobDescriptors = [];
    public IEnumerable<JobDescriptor> Jobs => jobDescriptors;

    public void ScheduleJob<TJob>(string name, params Condition[] conditions) where TJob : IJob
    {
        jobDescriptors.Add(new JobDescriptor(name, typeof(TJob), conditions, Guid.NewGuid()));
    }
}
