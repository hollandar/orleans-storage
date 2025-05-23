﻿using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;

namespace Webefinity.Module.Messaging.Mailkit
{
    public class MailkitEmailSender : IEmailSender
    {
        private readonly SmtpOptions smtpOptions;
        private readonly ILogger<MailkitEmailSender> logger;

        public MailkitEmailSender(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<ISmtpOptionsProvider>();
            if (options is null)
            {
                throw new InvalidOperationException("The SMTP options provider is not registered");
            }

            this.smtpOptions = options.GetSmtpOptions();

            // Validate the options
            if (string.IsNullOrWhiteSpace(this.smtpOptions.Host))
            {
                throw new ArgumentException("The SMTP server host is not set", nameof(this.smtpOptions.Host));
            }

            this.logger = serviceProvider.GetRequiredService<ILogger<MailkitEmailSender>>();
        }

        public Task SendAsync(EmailMessageModel emailMessage, CancellationToken? ct)
        {
            // Create a new email message
            var message = new MimeMessage();
            foreach (var address in emailMessage.To)
            {
                message.To.Add(new MailboxAddress(address.Name, address.Address));
            }
            foreach (var address in emailMessage.Cc)
            {
                message.Cc.Add(new MailboxAddress(address.Name, address.Address));
            }
            foreach (var address in emailMessage.Bcc)
            {
                message.Bcc.Add(new MailboxAddress(address.Name, address.Address));
            }
            if (emailMessage.From is not null)
            {
                message.From.Add(new MailboxAddress(emailMessage.From.Name, emailMessage.From.Address));
            }
            if (!message.From.Any())
            {
                message.From.Add(new MailboxAddress(smtpOptions.FromName, smtpOptions.From));
            }

            message.Subject = emailMessage.Subject;

            // Add the email body
            switch (emailMessage.Format)
            {
                case EmailMessageFormat.Text:
                    message.Body = new TextPart("plain") { Text = emailMessage.Body };
                    break;
                case EmailMessageFormat.Html:
                    message.Body = new TextPart("html") { Text = emailMessage.Body };
                    break;
                case EmailMessageFormat.Markdown:
                    message.Body = new TextPart("markdown") { Text = Markdig.Markdown.ToHtml(emailMessage.Body) };
                    break;
                default:
                    throw new ArgumentException($"Email format is not known {emailMessage.Format}", nameof(emailMessage.Format));
            }

            // Connect to the SMTP server and send the email
            using (var client = new SmtpClient())
            {
                try
                {
                    // Connect to the SMTP server
                    client.Connect(this.smtpOptions.Host, this.smtpOptions.Port, this.smtpOptions.UseSsl);

                    // Authenticate if needed
                    if (this.smtpOptions.RequiresAuthentication)
                    {
                        client.Authenticate(this.smtpOptions.Username, this.smtpOptions.Password);
                    }

                    // Send the email
                    client.Send(message);

                    this.logger?.LogInformation("MailKit: Email sent to {To} with subject {Subject}", message.To, message.Subject);
                }
                catch (Exception ex)
                {
                    this.logger?.LogError("MailKit: Email sent to {To} with subject {Subject} failed: {Error}", message.To, message.Subject, ex.Message);
                    throw new MessagingException($"An error occurred while sending the email: {ex.Message}");
                }
                finally
                {
                    // Disconnect from the SMTP server
                    client.Disconnect(true);
                }
            }

            return Task.CompletedTask;
        }
    }
}
