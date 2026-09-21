using Webefinity.Module.Scheduler.Configuration;
using Webefinity.Module.Scheduler.Interfaces;
using Webefinity.Module.Scheduler.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Webefinity.Module.Scheduler;

public static class StartupExtensions
{
    public static void AddScheduler(this IServiceCollection services, Action<JobDescriptorConfigurationOptions> configureOptions)
    {
        // Add scheduler services
        services.AddScoped<IJobManualTrigger, JobManualTriggerService>();
        services.AddHostedService<SchedulerBackgroundWorker>();

        // Configure scheduled options for the scheduler
        JobDescriptorConfigurationOptions options = new();
        configureOptions(options);

        // Register the individual job types as singletons in the DI container
        HashSet<Type> alreadyRegistered = [];
        foreach (var job in options.Jobs)
        {
            if (alreadyRegistered.Contains(job.JobType))
            {
                continue; // Skip if already registered
            }

            alreadyRegistered.Add(job.JobType);
            services.AddScoped(job.JobType);
        }

        // Push the configured job options in an options wrapper to the DI container
        services.AddSingleton<IOptions<JobDescriptorConfigurationOptions>>(Microsoft.Extensions.Options.Options.Create(options));
    }
}
