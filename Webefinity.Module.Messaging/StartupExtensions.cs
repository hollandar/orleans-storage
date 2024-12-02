using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Modules.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public static class StartupExtensions
{
    public static IServiceCollection ConfigureMessaging(this IServiceCollection services)
    {
        services.TryAddScoped<IEmailSender, EmailLoggingService>();
        services.TryAddScoped<IMessagingActive, AlwaysMessagingActiveService>();
        services.AddScoped<ISenderTransportService, SenderTransportService>();
        services.AddScoped<IMessengerService, MessengerService>();
        services.AddHostedService<MessagingSenderService>();
        return services;
    }
}
