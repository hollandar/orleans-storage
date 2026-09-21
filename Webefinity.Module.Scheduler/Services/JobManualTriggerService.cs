using Webefinity.Module.Scheduler.Conditions;
using Webefinity.Module.Scheduler.Configuration;
using Webefinity.Module.Scheduler.Interfaces;
using Microsoft.Extensions.Options;

namespace Webefinity.Module.Scheduler.Services;

internal class JobManualTriggerService(IOptions<JobDescriptorConfigurationOptions> options) : IJobManualTrigger
{
    private readonly IOptions<JobDescriptorConfigurationOptions> options = options;

    public void TriggerJob(string triggerName)
    {
        options.Value.Jobs.FirstOrDefault(j => j.Conditions.Any(r => r is ConditionManual conditionManual && conditionManual.TriggerName == triggerName))?.TriggeredManually = true;
    }
}

