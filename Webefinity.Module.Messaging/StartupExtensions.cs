﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Webefinity.Module.Messaging.Abstractions;

namespace Webefinity.Module.Messaging;

public static class StartupExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, string? emailSenderKey = null, string? smsSenderKey = null)
    {
        services.TryAddKeyedScoped<ISmsSender, SmsLoggingService>(smsSenderKey ?? Constants.SmsLoggingService);
        services.TryAddKeyedScoped<IEmailSender, EmailLoggingService>(emailSenderKey ?? Constants.EmailLoggingService);
        services.TryAddScoped<IMessagingActive, AlwaysMessagingActiveService>();
        services.AddScoped<ISenderTransportService, SenderTransportService>();
        services.AddScoped<IMessengerService, MessengerService>();
        return services;
    }
    
    /// <summary>
    /// Messaging host is optional, if it is not added you should arrange for SenderTransportService to be called manually, periodically.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddMessagingHost(this IServiceCollection services)
    {
        services.AddHostedService<MessagingSenderService>();
        return services;
    }
}
