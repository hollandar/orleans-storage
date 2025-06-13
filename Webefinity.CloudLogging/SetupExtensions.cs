using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using Webefinity.CloudLogging.Options;
using LoggingOptions = Webefinity.CloudLogging.Options.LoggingOptions;

namespace Webefinity.CloudLogging;

public static class SetupExtensions
{
    public static void AddWebefinityCloudLogging(this WebApplicationBuilder builder, string? configurationName = null)
    {
        configurationName ??= "CloudLogging";

        // Bind CloudLogging logging options
        var loggingOptions = builder.Configuration.GetSection("CloudLogging").Get<LoggingOptions>() ?? new LoggingOptions();

        var environmentName = builder.Environment.EnvironmentName;
        if (!loggingOptions.Environments.ContainsKey(environmentName))
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole().SetMinimumLevel(LogLevel.Debug);
            });

            return;
        }

        var environment = loggingOptions.Environments[environmentName];
        if (environment.CloudWatch != null)
        {
            // Configure AWS CloudWatch logging
            var cloudWatchOptions = environment.CloudWatch;
            var awsCredentials = new BasicAWSCredentials(cloudWatchOptions.AccessKeyId, cloudWatchOptions.SecretAccess);
            var region = RegionEndpoint.GetBySystemName(cloudWatchOptions.Region);
            var cloudWatchClient = new AmazonCloudWatchLogsClient(awsCredentials, region);
            var options = new CloudWatchSinkOptions
            {
                LogGroupName = cloudWatchOptions.LogGroup,
                TextFormatter = new Serilog.Formatting.Compact.CompactJsonFormatter(),
                MinimumLogEventLevel = cloudWatchOptions.MinimumLevel
            };
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(cloudWatchOptions.MinimumLevel);
            if (cloudWatchOptions.WriteTo.Contains(LogTargets.Console))
            {
                loggerConfiguration.WriteTo.Console();
            }
            if (cloudWatchOptions.WriteTo.Contains(LogTargets.CloudWatch))
            {
                loggerConfiguration.WriteTo.AmazonCloudWatch(options, cloudWatchClient);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }
        else if (environment.Local != null)
        {
            // Configure local console logging
            var localOptions = environment.Local;
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(localOptions.MinimumLevel);
            if (localOptions.WriteTo.Contains(LogTargets.Console))
            {
                loggerConfiguration.WriteTo.Console();
            }

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        builder.Host.UseSerilog();
    }
}