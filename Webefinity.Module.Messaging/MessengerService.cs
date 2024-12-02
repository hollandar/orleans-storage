using Microsoft.Extensions.Options;
using Webefinity.Module.Messaging.Abstractions.Args;
using Webefinity.Module.Messaging.Data;
using Webefinity.Module.Messaging.Options;
using Webefinity.Results;

namespace Webefinity.Module.Messaging;

internal class MessengerService: IMessengerService
{
    private readonly MessagingDbContext dbContext;
    private readonly IOptions<MessagingOptions> options;

    public MessengerService(MessagingDbContext dbContext, IOptions<MessagingOptions> options)
    {
        this.dbContext = dbContext;
        this.options = options;
    }

    public async Task<ValueResult<Guid>> QueueMessageAsync(EmailMessageModel emailMessage)
    {
        try
        {
            var message = new Message
            {
                Target = MessageTarget.Email,
                Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                Subject = emailMessage.Subject,
                Content = emailMessage.Body,
                Format = MapEmailFormat(emailMessage.Format),
                PurgeAfter = DateTimeOffset.UtcNow.AddDays(options.Value.PurgeAfterDays),
                Status = SendStatus.New
            };
            dbContext.Messages.Add(message);

            foreach (var to in emailMessage.To)
            {
                var address = new Address
                {
                    Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                    Email = to.address,
                    Name = to.name,
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
                    Email = cc.address,
                    Name = cc.name,
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
                    Email = bcc.address,
                    Name = bcc.name,
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

    public async Task<ValueResult<Guid>> QueueMessageAsync(SmsMessageModel smsMessageModel)
    {
        try
        {
            var message = new Message
            {
                Id = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql),
                Target = MessageTarget.SMS,
                Content = smsMessageModel.Message,
                Format = MessageFormat.Text,
                PurgeAfter = DateTimeOffset.UtcNow.AddDays(options.Value.PurgeAfterDays),
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
}
