using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Args;
using Webefinity.Module.Messaging.Options;

namespace Webefinity.Module.Messaging.Mailkit
{
    public class MailkitEmailSender : IEmailSender
    {
        private readonly IOptions<MailkitServerOptions> options;

        public MailkitEmailSender(IOptions<MailkitServerOptions> options)
        {
            this.options = options;

            // Validate the options
            if (string.IsNullOrWhiteSpace(this.options.Value.Host))
            {
                throw new ArgumentException("The SMTP server host is not set", nameof(this.options.Value.Host));
            }
        }

        public Task SendAsync(EmailMessageModel emailMessage, CancellationToken? ct)
        {
            // Create a new email message
            var message = new MimeMessage();
            foreach (var address in emailMessage.To)
            {
                message.To.Add(new MailboxAddress(address.name, address.address));
            }
            foreach (var address in emailMessage.Cc)
            {
                message.Cc.Add(new MailboxAddress(address.name, address.address));
            }
            foreach (var address in emailMessage.Bcc)
            {
                message.Bcc.Add(new MailboxAddress(address.name, address.address));
            }
            if (emailMessage.From is not null)
            {
                message.From.Add(new MailboxAddress(emailMessage.From.name, emailMessage.From.address));
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
                    client.Connect(this.options.Value.Host, this.options.Value.Port, this.options.Value.UseSsl);

                    // Authenticate if needed
                    if (this.options.Value.RequiresAuthentication)
                    {
                        client.Authenticate(this.options.Value.Username, this.options.Value.Password);
                    }

                    // Send the email
                    client.Send(message);

                }
                catch (Exception ex)
                {
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
