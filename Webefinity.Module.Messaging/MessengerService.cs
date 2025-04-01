using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Webefinity.Module.Messaging.Abstractions;
using Webefinity.Module.Messaging.Abstractions.Models;
using Webefinity.Module.Messaging.Data;
using Webefinity.Module.Messaging.Options;
using Webefinity.Results;

namespace Webefinity.Module.Messaging;

internal class MessengerService : IMessengerService
{
    private readonly IMessagingDbContext dbContext;
    private readonly IOptions<MessagingOptions> options;

    public MessengerService(IMessagingDbContext dbContext, IOptions<MessagingOptions> options)
    {
        this.dbContext = dbContext;
        this.options = options;
    }

    public async Task<ValueResult<Guid>> QueueMessageAsync(string senderId, EmailMessageModel emailMessage, CancellationToken ct = default)
    {
        try
        {
            var validator = new EmailMessageModelValidator();
            var validationResult = validator.Validate(emailMessage);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Email message is not valid {String.Join(", ", validationResult.Errors.Select(r => r.ErrorMessage))}", nameof(emailMessage));
            }

            var message = new Message
            {
                Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                SenderId = senderId,
                Target = MessageTarget.Email,
                Subject = emailMessage.Subject,
                Content = emailMessage.Body,
                Format = MapEmailFormat(emailMessage.Format),
                PurgeAfter = DateTimeOffset.UtcNow.AddDays(options.Value.PurgeAfterDays),
                Status = SendStatus.New,
                RetryCount = this.options.Value.RetryCount,
                RetryAfter = null
            };
            dbContext.Messages.Add(message);

            foreach (var to in emailMessage.To)
            {
                var address = new Address
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Email = to.Address,
                    Name = to.Name,
                    Type = AddressType.To,

                };
                dbContext.Addresses.Add(address);
                message.Addresses.Add(address);
            }

            foreach (var cc in emailMessage.Cc)
            {
                var address = new Address
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Email = cc.Address,
                    Name = cc.Name,
                    Type = AddressType.Cc,

                };
                dbContext.Addresses.Add(address);
                message.Addresses.Add(address);
            }

            foreach (var bcc in emailMessage.Bcc)
            {
                var address = new Address
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Email = bcc.Address,
                    Name = bcc.Name,
                    Type = AddressType.Bcc,

                };
                dbContext.Addresses.Add(address);
                message.Addresses.Add(address);
            }

            foreach (var attachment in emailMessage.Attachments)
            {
                var attachmentData = new Attachment
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Name = attachment.Name,
                    Data = attachment.Data,
                    ContentType = attachment.ContentType,
                    Length = attachment.Length,
                };

                dbContext.Attachments.Add(attachmentData);
                message.Attachments.Add(attachmentData);
            }

            message.Status = SendStatus.Pending;
            await dbContext.SaveChangesAsync();

            return ValueResult<Guid>.Ok(message.Id);

        }
        catch (ArgumentException ex)
        {
            return ValueResult<Guid>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ValueResult<Guid>.Fail(ex.Message);
        }
    }

    private MessageFormat MapEmailFormat(EmailMessageFormat format)
    {
        return format switch
        {
            EmailMessageFormat.Html => MessageFormat.Html,
            EmailMessageFormat.Text => MessageFormat.Text,
            EmailMessageFormat.Markdown => MessageFormat.Markdown,
            _ => throw new ArgumentException($"Invalid email format {format.ToString()}", nameof(format)),
        };
    }

    public async Task<ValueResult<Guid>> QueueMessageAsync(string senderId, SmsMessageModel smsMessageModel, CancellationToken ct = default)
    {
        try
        {
            var message = new Message
            {
                Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                SenderId = senderId,
                Target = MessageTarget.SMS,
                Content = smsMessageModel.Message,
                Format = MessageFormat.Text,
                PurgeAfter = DateTimeOffset.UtcNow.AddDays(options.Value.PurgeAfterDays),
                Status = SendStatus.New,
                RetryCount = this.options.Value.RetryCount,
                RetryAfter = null

            };
            dbContext.Messages.Add(message);

            foreach (var to in smsMessageModel.To)
            {
                var address = new Address
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Phone = to,
                    Type = AddressType.To,
                };
                dbContext.Addresses.Add(address);
                message.Addresses.Add(address);
            }

            message.Status = SendStatus.Pending;
            await dbContext.SaveChangesAsync();
            return ValueResult<Guid>.Ok(message.Id);
        }
        catch (ArgumentException ex)
        {
            return ValueResult<Guid>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ValueResult<Guid>.Fail(ex.Message);
        }

    }

    public Task<PaginatedContainer<MessageListModel>> ListMessagesAsync(PageRequest pageRequest, CancellationToken ct = default)
    {
        var messages = this.dbContext.Messages.OrderByDescending(r =>  r.Created).AsQueryable();
        var total = messages.Count();
        if (!String.IsNullOrEmpty(pageRequest.Search))
        {
            messages = messages.Where(r => r.Content.Contains(pageRequest.Search) || (r.Subject != null && r.Subject.Contains(pageRequest.Search)));
        }

        var pagedMessages = messages.Skip(pageRequest.Skip).Take(pageRequest.Take).ToList();
        var models = pagedMessages.Select(r =>
        {
            this.dbContext.Entry(r).Collection(r => r.Addresses).Load();
            var to = String.Join(", ", r.Addresses.Where(r => r.Type == AddressType.To).Select(a => {
                if (!String.IsNullOrWhiteSpace(a.Phone))
                {
                    return a.Phone;
                } else
                {
                    return $"{a.Email} <{a.Name}>";
                }
            }));
            return new MessageListModel
            {
                Id = r.Id,
                To = to,
                Subject = r.Subject,
                Content = r.Content,
                Created = r.Created,
                Sent = r.Sent,
                Status = r.Status,
                Target = r.Target,
                SenderId = r.SenderId
            };
        }).ToList();

        return Task.FromResult(new PaginatedContainer<MessageListModel>(models, total));
    }

    public Task<ValueResult<MessageModel>> GetMessageAsync(Guid messageId, CancellationToken ct = default)
    {
        var message = this.dbContext.Messages.Include(r => r.Addresses).Include(r => r.Attachments).Where(r => r.Id == messageId).SingleOrDefault();
        if (message is null)
        {
            return Task.FromResult(ValueResult<MessageModel>.Fail("Message not found.", ResultReasonType.NotFound));
        }

        var model = new MessageModel
        {
            Id = message.Id,
            Subject = message.Subject,
            Content = message.Content,
            Addresses = message.Addresses.Select(r => new AddressModel
            {
                Id = r.Id,
                Email = r.Email,
                Phone = r.Phone,
                Name = r.Name,
                Type = r.Type
            }).ToList(),
            Attachments = message.Attachments.Select(r => new AttachmentModel
            {
                Id = r.Id,
                Name = r.Name,
                ContentType = r.ContentType,
                Length = r.Length
            }).ToList(),
            Status = message.Status,
            Format = message.Format,
            Created = message.Created,
            Error = message.Error,
            Sent = message.Sent,
            PurgeAfter = message.PurgeAfter,
            Target = message.Target,
            SenderId = message.SenderId
        };

        return Task.FromResult(ValueResult<MessageModel>.Ok(model));
    }
}
