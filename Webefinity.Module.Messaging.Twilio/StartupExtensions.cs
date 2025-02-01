using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging.Twilio;

public static class StartupExtensions
{
    public static IServiceCollection AddTwilioSmsSender(this IServiceCollection services)
    {
        services.TryAddKeyedScoped<ISmsSender, TwilioSmsSender>(Constants.SmsTwilioService);

        return services;
    }
}
