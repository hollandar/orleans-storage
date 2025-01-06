using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Webefinity.Module.Messaging.Options;

namespace Webefinity.Module.Messaging;

public class MessagingSenderService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<MessagingOptions> options;
    private int waitTime = 1000;
    private DateTimeOffset purgeAfter = DateTimeOffset.MinValue;

    public MessagingSenderService(IServiceProvider serviceProvider, IOptions<MessagingOptions> options)
    {
        this.serviceProvider = serviceProvider;
        this.options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<MessagingSenderService>>();
        var senderTransportService = scope.ServiceProvider.GetRequiredService<ISenderTransportService>();
        var messagingActive = scope.ServiceProvider.GetRequiredService<IMessagingActive>();

        logger?.LogInformation("MessagingSenderService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!await messagingActive.IsMessagingAsync())
            {
                if (waitTime < this.options.Value.MaxWaitTime)
                {
                    waitTime += this.options.Value.WaitTime;
                }

                await Task.Delay(waitTime, stoppingToken);
                continue;
            }

            try
            {
                if (purgeAfter < DateTimeOffset.UtcNow)
                {
                    await senderTransportService.PurgeAsync(stoppingToken);
                    purgeAfter = DateTimeOffset.UtcNow.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Could not purge old messages.");
            }

            try
            {
                var workDone = await senderTransportService.SendAsync(stoppingToken);
                
                if (workDone == 0 && waitTime < this.options.Value.MaxWaitTime)
                {
                    waitTime += this.options.Value.WaitTime;
                }
                else if (workDone > 0 && waitTime > this.options.Value.WaitTime)
                {
                    waitTime = this.options.Value.WaitTime;
                }
                await Task.Delay(waitTime, stoppingToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred during sending messages.");
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<MessagingSenderService>>(); 
        logger?.LogInformation("MessagingSenderService is stopping.");
        return base.StopAsync(cancellationToken);
    }
}
