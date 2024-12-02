namespace Webefinity.Module.Messaging.Abstractions.Args;

public enum EmailMessageFormat { None, Text, Html, Markdown }
public record EmailAddress(string address, string name);

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

public class EmailAttachmentModel
{
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; } = 0;
}
