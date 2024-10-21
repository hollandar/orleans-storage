using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.MessageBus;

public static class StartupExtensions
{
    public static IServiceCollection ConfigureMessageBus(this IServiceCollection services, Action<IBusOptions> options)
    {

        var busOptions = new BusOptions();
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
