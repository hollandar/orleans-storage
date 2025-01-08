using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;

namespace Webefinity.Module.Messaging
{
    public class EmailLoggingService : IEmailSender
    {
        private IServiceProvider serviceProvider;
        private ILogger<EmailLoggingService>? logger;

        public EmailLoggingService(IServiceProvider serviceProvider) {
            this.serviceProvider = serviceProvider;
            this.logger = serviceProvider.GetService<ILogger<EmailLoggingService>>();
        }

        public Task SendAsync(EmailMessageModel emailMessage, CancellationToken? ct)
        {
            foreach (var to in emailMessage.To)
            {
                logger?.LogInformation($"Sending email to {to.Address}");
            }

            logger?.LogInformation($"Email subject: {emailMessage.Subject}");
            logger?.LogInformation($"Email body: {emailMessage.Body}");

            logger?.LogWarning("Email was not sent, only logged.  Add a different IEmailSender implementation to send emails.");

            return Task.CompletedTask;
        }
    }
}
