using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging.Mailkit;

public static class StartupExtensions
{
    public static IServiceCollection AddMailkitEmailSender(this IServiceCollection services)
    {
        services.TryAddKeyedScoped<IEmailSender, MailkitEmailSender>(Constants.EmailSmtpService);

        return services;
    }
}
