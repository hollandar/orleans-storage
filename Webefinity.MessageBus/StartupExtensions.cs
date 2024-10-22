using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.MessageBus;

public static class StartupExtensions
{
    public static IServiceCollection ConfigureMessageBus(this IServiceCollection services, Action<IMessageBusOptions> options)
    {

        var busOptions = new MessageBusOptions();
        options(busOptions);

        services.AddSingleton(busOptions);
        services.AddSingleton<MessageBus>();

        foreach (var subscriberType in busOptions.GetSubscriberTypes())
        {
            services.AddScoped(subscriberType);
        }

        return services;
    }
}
