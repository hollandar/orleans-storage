using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Modules.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public class MessagingSenderService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private int waitTime = 1000;
    private DateTimeOffset purgeAfter = DateTimeOffset.MinValue;

    public MessagingSenderService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
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
                if (waitTime < 10000)
                {
                    waitTime += 1000;
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
                
                if (workDone == 0 && waitTime < 10000)
                {
                    waitTime += 1000;
                }
                else if (workDone > 0 && waitTime > 1000)
                {
                    waitTime = 1000;
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
