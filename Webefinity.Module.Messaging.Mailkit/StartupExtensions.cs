using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging.Mailkit;

public static class StartupExtensions
{
    public static IServiceCollection AddMailkitEmailSender(this IServiceCollection services, string? key = null)
    {
        services.TryAddKeyedScoped<IEmailSender, MailkitEmailSender>(key ?? Constants.EmailSmtpService);

        return services;
    }
}
