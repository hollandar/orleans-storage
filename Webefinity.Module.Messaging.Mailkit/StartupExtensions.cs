using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging.Mailkit;

public static class StartupExtensions
{
    public static IServiceCollection AddMailkitEmailSender(this IServiceCollection services)
    {
        services.AddSingleton<IEmailSender, MailkitEmailSender>();

        return services;
    }
}
