using Webefinity.Module.Scheduler.Conditions;

namespace Webefinity.Module.Scheduler.Configuration;

public record JobDescriptor(string Name, Type JobType, Condition[] Conditions, Guid Id)
{
    public bool TriggeredManually { get; set; } = false;
}
