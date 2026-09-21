using Webefinity.Module.Scheduler.Conditions;
using Webefinity.Module.Scheduler.Configuration;
using Webefinity.Module.Scheduler.Interfaces;
using Webefinity.Module.Scheduler.Options;
using Webefinity.Module.Scheduler.Services.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Webefinity.Module.Scheduler.Services;

public record FailTime(DateTimeOffset Time, double BackoffMinutes);

internal class SchedulerBackgroundWorker(IServiceProvider serviceProvider) : BackgroundService
{
    private readonly IOptions<JobDescriptorConfigurationOptions> jobDescriptorOptions = serviceProvider.GetRequiredService<IOptions<JobDescriptorConfigurationOptions>>();
    private readonly Dictionary<Guid, DateOnly> lastRunDates = [];
    private readonly Dictionary<Guid, DateTimeOffset> lastRunTimes = [];
    private readonly Dictionary<Guid, FailTime> lastFailTimes = [];
    private readonly ILogger<SchedulerBackgroundWorker>? logger = serviceProvider.GetService<ILogger<SchedulerBackgroundWorker>>();
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IOptions<SchedulerOptions> schedulerOptions = serviceProvider.GetRequiredService<IOptions<SchedulerOptions>>();
    private readonly SemaphoreSlim runningSemaphore = new(1, 1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var jobSchedulerActive = scope.ServiceProvider.GetService<IJobSchedulerActive>();
            await Task.Delay(TimeSpan.FromSeconds(this.schedulerOptions.Value.CycleIntervalSeconds), stoppingToken);
            if (jobSchedulerActive is not null && await jobSchedulerActive.IsJobSchedulerActiveAsync(stoppingToken))
                try
                {
                    if (!await runningSemaphore.WaitAsync(1000, stoppingToken))
                    {
                        logger?.LogWarning("Previous job execution is still running. Skipping this cycle.");
                        continue;
                    }

                    await ExecuteTickAsync(DateTimeOffset.UtcNow, scope.ServiceProvider, stoppingToken);
                }
                finally
                {
                    runningSemaphore.Release();
                }
        }
    }

    private async Task ExecuteTickAsync(DateTimeOffset currentTime, IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        foreach (var jobDescriptor in this.jobDescriptorOptions.Value.Jobs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            var lastRunTime = lastRunTimes.TryGetValue(jobDescriptor.Id, out DateTimeOffset lrt) ? lrt : DateTimeOffset.MinValue;
            var lastRunDate = lastRunDates.TryGetValue(jobDescriptor.Id, out DateOnly lrd) ? lrd : DateOnly.MinValue;
            var lastFailTime = lastFailTimes.TryGetValue(jobDescriptor.Id, out FailTime? ft) ? ft : new FailTime(DateTimeOffset.MinValue, 1);

            DateTimeOffset scheduledTime = currentTime;
            bool shouldRun = false;
            TimeSpan? interval = null;
            foreach (var condition in jobDescriptor.Conditions)
            {
                switch (condition)
                {
                    case ConditionScheduledTime conditionScheduledTime:
                        if (currentTime.TimeOfDay >= conditionScheduledTime.ScheduledTime.ToTimeSpan() && lastRunDate < DateOnly.FromDateTime(currentTime.Date))
                        {
                            shouldRun = true;
                            scheduledTime = currentTime.Date + conditionScheduledTime.ScheduledTime.ToTimeSpan();
                        }
                        break;
                    case ConditionEvery every:
                        if (lastRunTime == DateTimeOffset.MinValue || currentTime - lastRunTime >= every.TimeSpan)
                        {
                            shouldRun = true;
                            interval = every.TimeSpan;
                        }
                        break;
                    case ConditionManual manual:
                        shouldRun = jobDescriptor.TriggeredManually;
                        break;
                }
            }
            if (shouldRun)
            {
                try
                {
                    if (lastFailTimes.ContainsKey(jobDescriptor.Id) && currentTime - lastFailTime.Time < TimeSpan.FromMinutes(lastFailTime.BackoffMinutes))
                    {
                        if (logger?.IsEnabled(LogLevel.Debug) == true)
                        {
                            logger.LogDebug("Job {JobName}:{JobId} failed recently. Skipping execution to avoid repeated failures.", jobDescriptor.Name, jobDescriptor.Id);
                        }
                        continue;
                    }

                    if (serviceProvider.GetRequiredService(jobDescriptor.JobType) is IJob jobInstance)
                    {
                        var context = new JobExecutionContext
                        {
                            JobName = jobDescriptor.Name,
                            ScheduledTime = scheduledTime,
                            FireTimeUtc = currentTime,
                            LastRunTime = lastRunTimes.TryGetValue(jobDescriptor.Id, out DateTimeOffset value) ? value : null,
                            Interval = interval,
                            CancellationToken = stoppingToken,
                        };
                        await jobInstance.ExecuteAsync(context);

                        // Update the last run time and date after execution
                        lastRunTimes[jobDescriptor.Id] = currentTime;
                        lastRunDates[jobDescriptor.Id] = DateOnly.FromDateTime(currentTime.Date);
                        jobDescriptor.TriggeredManually = false; // Reset the manual trigger after execution
                        lastFailTimes.Remove(jobDescriptor.Id); // Clear the last fail time on successful execution
                    }
                    else
                    {
                        if (this.logger?.IsEnabled(LogLevel.Error) == true)
                        {
                            this.logger.LogError("Job type {JobType} for job {JobName}:{JobId} could not be resolved.", jobDescriptor.JobType.FullName, jobDescriptor.Name, jobDescriptor.Id);
                        }
                    }
                }

                catch (Exception ex)
                {
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                    {
                        logger.LogError(ex, "Error executing job {JobName}:{JobId}", jobDescriptor.Name, jobDescriptor.Id);
                    }
                    lastFailTimes[jobDescriptor.Id] = lastFailTimes.TryGetValue(jobDescriptor.Id, out FailTime? value)
                        ? new FailTime(value.Time, Math.Min(this.schedulerOptions.Value.BackoffMaximumMinutes, value.BackoffMinutes * this.schedulerOptions.Value.BackoffFactor))
                        : new FailTime(currentTime, 1);
                }
            }
        }
    }
}
