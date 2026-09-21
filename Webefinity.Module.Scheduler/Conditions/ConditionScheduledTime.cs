namespace Webefinity.Module.Scheduler.Conditions;

public record ConditionScheduledTime(TimeOnly ScheduledTime) : Condition;
