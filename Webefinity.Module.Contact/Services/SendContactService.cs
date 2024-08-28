using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Webefinity.Module.Contact.Interfaces;
using Webefinity.Module.Contact.Models;
using Webefinity.Module.Contact.Options;
using Webefinity.Results;
using Webefinity.Validation;
using static System.Formats.Asn1.AsnWriter;

namespace Webefinity.Module.Contact.Services;

public class SendContactService : ISendContactService
{
    private readonly IOptions<SmtpOptions> smtpOptions;
    private readonly ILogger<SendContactService> logger;
    private readonly IServiceProvider serviceProvider;

    public SendContactService(IOptions<SmtpOptions> smtpOptions, ILogger<SendContactService> logger, IServiceProvider serviceProvider)
    {
        this.smtpOptions = smtpOptions;
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public async Task<Result> SendWithFormatterAsync<TModelType>(TModelType model, string? type = null) {
        using var scope = this.serviceProvider.CreateScope();
        var formatters = scope.ServiceProvider.GetServices<IContactMessageFormatter>();
        var typeName = type ?? typeof(TModelType).FullName!;
        var formatter = formatters.Where(f => f.HandlesType(typeName)).FirstOrDefault();
        if (formatter is null)
        {
            logger.LogError("Could not handle contact type {ContactType}", typeName);
            throw new InvalidOperationException($"No formatter available for {typeName}.");
        }

        var contactModel = new ContactModel
        {
            Id = Guid.NewGuid(),
            Body = JsonSerializer.Serialize(model),
            Type = typeName,
        };
        var formattedMessage = formatter.FormatMessage([contactModel]);

        return await SendAsync(formattedMessage);
    }

    public async Task<Result> SendAsync(MailMessageModel sendMessage)
    {
        var validator = new MailMessageModelValidator();
        var validationResult = validator.Validate(sendMessage);
        if (!validationResult.IsValid)
        {
            return validationResult.AsResult();
        }

        try
        {
            using var smtpClient = new SmtpClient(smtpOptions.Value.Host, smtpOptions.Value.Port);
            smtpClient.Credentials = new NetworkCredential(smtpOptions.Value.Username, smtpOptions.Value.Password);
            smtpClient.EnableSsl = smtpOptions.Value.EnableSSL;

            var mailMessage = new MailMessage(sendMessage.From, sendMessage.To);
            mailMessage.Subject = sendMessage.Subject;
            mailMessage.Body = sendMessage.Format switch
            {
                BodyFormat.Md => Markdig.Markdown.ToHtml(sendMessage.Body),
                BodyFormat.Text => sendMessage.Body,
                BodyFormat.Html => sendMessage.Body,
                _ => throw new InvalidOperationException("Bodyformat is not supported.")
            };
            mailMessage.IsBodyHtml = sendMessage.Format switch
            {
                BodyFormat.Md => true,
                BodyFormat.Html => true,
                _ => false
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Could not send email.");
            return Result.Fail("Could not send email.");
        }

        return Result.Ok();
    }
}
