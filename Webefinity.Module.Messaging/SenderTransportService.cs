using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;
using Webefinity.Module.Messaging.Data;

namespace Webefinity.Module.Messaging;

public class SenderTransportService : ISenderTransportService
{
    private readonly MessagingDbContext dbContext;
    private readonly IServiceProvider serviceProvider;

    public SenderTransportService(MessagingDbContext dbContext, IServiceProvider serviceProvider)
    {
        this.dbContext = dbContext;
        this.serviceProvider = serviceProvider;
    }

    public async Task<int> SendAsync(CancellationToken ct = default)
    {
        int workDone = 0;
        while (!ct.IsCancellationRequested)
        {
            if (!this.dbContext.Messages.Where(r => r.Status == SendStatus.Pending).Any())
            {
                // No work to be done in this cycle
                break;
            }

            var message = this.dbContext.Messages.Where(r => r.Status == SendStatus.Pending).OrderBy(r => r.Created).First();
            try
            {
                switch (message.Target)
                {
                    case MessageTarget.Email:
                        await SendEmailAsync(message, ct);
                        break;
                    case MessageTarget.SMS:
                        await SendSmsAsync(message, ct);
                        break;
                    case MessageTarget.PushNotification:
                        await SendPushAsync(message, ct);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                message.Status = SendStatus.Sent;
                message.Sent = DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(ct);
            } catch (Exception ex)
            {
                message.Status = SendStatus.Failed;
                message.Error = ex.Message;
                await dbContext.SaveChangesAsync(ct);
            }
        }

        return workDone;
    }

    private Task SendPushAsync(Message message, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    private async Task SendSmsAsync(Message message, CancellationToken ct = default)
    {
        var smsSender = this.serviceProvider.GetService<ISmsSender>();
        if (smsSender is null)
        {
            throw new InvalidOperationException("SmsSender service not registered");
        }

        if (message.Format != MessageFormat.Text)
        {
            throw new ArgumentException("SMS messages must be text format", nameof(message));
        }

        var smsMessage = new SmsMessageModel
        {
            Message = message.Content,
            To = message.Addresses.Where(r => r.Type == AddressType.To).Select(r => r.Phone).ToArray()
        };

        await smsSender.SendAsync(smsMessage, ct);
    }

    private async Task SendEmailAsync(Message message, CancellationToken ct = default)
    {
        var emailSender = this.serviceProvider.GetService<IEmailSender>();
        if (emailSender is null)
        {
            throw new InvalidOperationException("EmailSender service not registered");
        }

        var emailMessage = new EmailMessageModel
        {
            Subject = message.Subject,
            Body = message.Content,
            Format = MapEmailFormat(message.Format),
            To = message.Addresses.Where(r => r.Type == AddressType.To).Select(r => new EmailAddress(r.Email, r.Name)).ToArray(),
            Cc = message.Addresses.Where(r => r.Type == AddressType.Cc).Select(r => new EmailAddress(r.Email, r.Name)).ToArray(),
            Bcc = message.Addresses.Where(r => r.Type == AddressType.Bcc).Select(r => new EmailAddress(r.Email, r.Name)).ToArray(),
            Attachments = message.Attachments.Select(r => new EmailAttachmentModel
            {
                Name = r.Name,
                Data = r.Data,
                ContentType = r.ContentType,
                Length = r.Length
            }).ToArray()
        };

        var validator = new EmailMessageModelValidator();
        var validationResult = validator.Validate(emailMessage);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException($"Email message is not valid {String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage))}", nameof(emailMessage));
        }

        await emailSender.SendAsync(emailMessage, ct);
    }

    private EmailMessageFormat MapEmailFormat(MessageFormat format)
    {
        return format switch { 
            MessageFormat.None => EmailMessageFormat.None, 
            MessageFormat.Text => EmailMessageFormat.Text, 
            MessageFormat.Html => EmailMessageFormat.Html, 
            MessageFormat.Markdown => EmailMessageFormat.Markdown, 
            _ => throw new ArgumentException($"Email format is not known {format}", nameof(format)) 
        };
    }

    public async Task PurgeAsync(CancellationToken ct = default)
    {
        var purgableMessages = this.dbContext.Messages.Where(r => r.PurgeAfter < DateTimeOffset.UtcNow);
        if (purgableMessages.Any())
        {
            await purgableMessages.ExecuteDeleteAsync(ct);
        }
    }
}
