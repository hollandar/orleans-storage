using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging.Twilio;

public static class StartupExtensions
{
    public static IServiceCollection AddTwilioSmsSender(this IServiceCollection services, string? key = null)
    {
        services.TryAddKeyedScoped<ISmsSender, TwilioSmsSender>(key ?? Constants.SmsTwilioService);

        return services;
    }
}
