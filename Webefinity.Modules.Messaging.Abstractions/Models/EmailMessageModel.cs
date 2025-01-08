using FluentValidation;

namespace Webefinity.Module.Messaging.Abstractions.Models;

public enum EmailMessageFormat { None, Text, Html, Markdown }
public record EmailAddress(string Address, string Name);

public class EmailAddressValidator : AbstractValidator<EmailAddress>
{
    public EmailAddressValidator()
    {
        RuleFor(r => r.Address).NotEmpty().EmailAddress();
        RuleFor(r => r.Name).NotEmpty();
    }
}

public class EmailMessageModel
{
    public EmailAddress[] To { get; set; } = Array.Empty<EmailAddress>();
    public EmailAddress[] Cc { get; set; } = Array.Empty<EmailAddress>();
    public EmailAddress[] Bcc { get; set; } = Array.Empty<EmailAddress>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailMessageFormat Format { get; set; } = EmailMessageFormat.Html;
    public EmailAddress? From { get; set; } = null;
    public EmailAttachmentModel[] Attachments { get; set; } = Array.Empty<EmailAttachmentModel>();
}
public class EmailMessageModelValidator : AbstractValidator<EmailMessageModel>
{
    public EmailMessageModelValidator()
    {
        //RuleFor(r => r.To).NotEmpty();
        RuleFor(r => r.To).ForEach(r => r.SetValidator(new EmailAddressValidator()));
        RuleFor(r => r.Cc).ForEach(r => r.SetValidator(new EmailAddressValidator()));
        RuleFor(r => r.Bcc).ForEach(r => r.SetValidator(new EmailAddressValidator()));
        RuleFor(r => r.Subject).NotEmpty();
        RuleFor(r => r.Body).NotEmpty();
        RuleFor(r => r.Format).IsInEnum().NotEqual(EmailMessageFormat.None);
        RuleFor(r => r.Attachments).ForEach(r => r.SetValidator(new EmailAttachmentModelValidator()));
    }
}

public class EmailAttachmentModel
{
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; } = 0;
}

public class EmailAttachmentModelValidator : AbstractValidator<EmailAttachmentModel>
{
    public EmailAttachmentModelValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.Data).NotEmpty();
        RuleFor(r => r.ContentType).NotEmpty();
        RuleFor(r => r.Length).GreaterThan(0);
    }
}
