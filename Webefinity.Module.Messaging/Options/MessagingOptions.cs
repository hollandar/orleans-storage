namespace Webefinity.Module.Messaging.Options;

public class MessagingOptions
{
    public int PurgeAfterDays { get; set; } = 365; // Days, one year
    public int WaitTime { get; set; } = 1000; // Milliseconds, 1 minute
    public int MaxWaitTime { get; set; } = 300000; // Milliseconds, 5 minutes
    public int RetryCount { get; set; } = 5;
    public int RetryDelay { get; set; } = 60; // Minutes
}
