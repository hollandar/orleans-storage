using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public static class StartupExtensions
{
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services)
    {
        services.TryAddKeyedScoped<ISmsSender, SmsLoggingService>(Constants.SmsLoggingService);
        services.TryAddKeyedScoped<IEmailSender, EmailLoggingService>(Constants.EmailLoggingService);
        services.TryAddScoped<IMessagingActive, AlwaysMessagingActiveService>();
        services.AddScoped<ISenderTransportService, SenderTransportService>();
        services.AddScoped<IMessengerService, MessengerService>();
        services.AddHostedService<MessagingSenderService>();
        return services;
    }
}
