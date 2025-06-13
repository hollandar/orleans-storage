using Serilog.Events;

namespace Webefinity.CloudLogging.Options;

public enum LogTargets
{
    Console,
    CloudWatch
}

public class LoggingOptions
{
    public IDictionary<string, EnvironmentOptions> Environments { get; set; } = new Dictionary<string, EnvironmentOptions>();
}

public class  EnvironmentOptions
{
    public CloudWatchOptions? CloudWatch { get; set; }
    public LogOptions? Local { get; set; }
}

public class CloudWatchOptions : LogOptions
{
    public string LogGroup { get; set; } = "LicenseConsoleLogs";
    public string Region { get; set; } = "ap-southeast-2";
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccess { get; set; } = string.Empty;
}

public class LogOptions
{
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    public HashSet<LogTargets> WriteTo { get; set; } = new HashSet<LogTargets> { LogTargets.Console };
}
