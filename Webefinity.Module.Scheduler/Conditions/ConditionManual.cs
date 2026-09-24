namespace Webefinity.Module.Scheduler.Conditions;

public record ConditionManual(string TriggerName) : Condition
{
    public bool IsTriggered { get; set; } = false;
}
