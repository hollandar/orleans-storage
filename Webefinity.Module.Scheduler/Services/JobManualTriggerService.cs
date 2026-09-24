using Webefinity.Module.Scheduler.Conditions;
using Webefinity.Module.Scheduler.Configuration;
using Webefinity.Module.Scheduler.Interfaces;
using Microsoft.Extensions.Options;

namespace Webefinity.Module.Scheduler.Services;

internal class JobManualTriggerService(IEnumerable<IOptions<JobDescriptorConfigurationOptions>> options) : IJobManualTrigger
{
    private readonly IEnumerable<IOptions<JobDescriptorConfigurationOptions>> options = options;

    public void TriggerJob(string triggerName)
    {
        var jobs = options.SelectMany(r => r.Value.Jobs);
        var conditions = jobs.SelectMany(r => r.Conditions).OfType<ConditionManual>();

        var triggerNames = conditions.Select(r => r.TriggerName).ToList();
        if (triggerNames.Count != triggerNames.ToHashSet().Count)
        {
            throw new InvalidOperationException($"Duplicate trigger names found: {string.Join(", ", triggerNames.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key))}");
        }

        conditions.Where(r => r.TriggerName == triggerName).ToList().ForEach(r => r.IsTriggered = true);
    }
}

