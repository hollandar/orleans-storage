using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;

namespace Webefinity.Module.Messaging
{
    public class SmsLoggingService : ISmsSender
    {
        private ILogger<SmsLoggingService>? logger;

        public SmsLoggingService(IServiceProvider serviceProvider)
        {
            this.logger = serviceProvider.GetService<ILogger<SmsLoggingService>>();
        }

        public Task SendAsync(SmsMessageModel smsMessage, CancellationToken ct)
        {
            foreach (var to in smsMessage.To)
            {
                logger?.LogInformation($"Sending sms to {to}");
            }

            logger?.LogInformation($"Sms body: {smsMessage.Message}");

            logger?.LogWarning("Sms was not sent, only logged.  Add a different ISmsSender implementation to send sms.");

            return Task.CompletedTask;
        }
    }
}
