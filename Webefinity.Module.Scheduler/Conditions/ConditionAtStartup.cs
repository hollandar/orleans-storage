using System;
using System.Collections.Generic;
using System.Text;

namespace Webefinity.Module.Scheduler.Conditions
{
    public record ConditionAtStartup():Condition()
    {
        public bool IsTriggered { get; set; } = true;
    }
}
