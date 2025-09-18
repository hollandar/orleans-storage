using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;
using Webefinity.Module.Messaging.Data;
using Webefinity.Module.Messaging.Options;

namespace Webefinity.Module.Messaging;

public class SenderTransportService : ISenderTransportService
{
    private readonly IMessagingDbContext dbContext;
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<MessagingOptions> options;

    public SenderTransportService(IMessagingDbContext dbContext, IServiceProvider serviceProvider, IOptions<MessagingOptions> options)
    {
        this.dbContext = dbContext;
        this.serviceProvider = serviceProvider;
        this.options = options;
    }

    public async Task<int> SendAsync(CancellationToken ct = default)
    {
        var guardDate = DateTimeOffset.UtcNow;

        var sendAvailableMessageCount = this.dbContext.Messages.Count(r =>
            r.Status == SendStatus.Pending ||
            (r.Status == SendStatus.Failed && r.RetryAfter != null && r.RetryAfter < guardDate && r.RetryCount > 0)
        );
        if (sendAvailableMessageCount == 0)
        {
            return 0;
        }

        var sendAvailableMessageQuery = this.dbContext.Messages
            .Include(r => r.Addresses)
            .Include(r => r.Attachments)
            .Where(r =>
                r.Status == SendStatus.Pending ||
                (r.Status == SendStatus.Failed && r.RetryAfter != null && r.RetryAfter < guardDate && r.RetryCount > 0)
            )
            .OrderBy(r => r.Created)
            .AsNoTracking();

        var sendQueue = new Queue<Message>(sendAvailableMessageQuery.ToList());

        int workDone = 0;
        while (!ct.IsCancellationRequested && sendQueue.Count > 0)
        {
            var message = sendQueue.Dequeue();
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

                dbContext.Messages.Where(r => r.Id == message.Id).ExecuteUpdate(
                    r => r.SetProperty(m => m.Status, SendStatus.Sent)
                          .SetProperty(m => m.Sent, DateTimeOffset.UtcNow)
                );
            } catch (Exception ex)
            {
                dbContext.Messages.Where(r => r.Id == message.Id).ExecuteUpdate(
                    r => r.SetProperty(m => m.Status, SendStatus.Failed)
                    .SetProperty(m => m.RetryAfter, guardDate.AddMinutes(this.options.Value.RetryDelay))
                    .SetProperty(m => m.RetryCount, r => r.RetryCount - 1)
                    .SetProperty(m => m.Error, ex.Message)
                );
            }
            finally
            {
                workDone++;
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
        var smsSender = this.serviceProvider.GetKeyedService<ISmsSender>(message.SenderId);
        if (smsSender is null)
        {
            smsSender = this.serviceProvider.GetKeyedService<ISmsSender>(Constants.SmsLoggingService);
        }

        if (smsSender is null)
        {
            throw new InvalidOperationException($"SmsSender service not registered for senderId {message.SenderId}.");
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
        var emailSender = this.serviceProvider.GetKeyedService<IEmailSender>(message.SenderId);
        if (emailSender is null)
        {
            emailSender = this.serviceProvider.GetKeyedService<IEmailSender>(Constants.EmailLoggingService);
        }

        if (emailSender is null)
        {
            throw new InvalidOperationException($"EmailSender service not registered {message.SenderId}.");
        }

        Debug.Assert(message.Subject is not null);
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
