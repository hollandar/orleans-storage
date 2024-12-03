

namespace Webefinity.Module.Messaging.Abstractions.Models;

public class MessageModel
{
    public object Addresses { get; set; }
    public string Content { get; set; }
    public string? Subject { get; set; }
    public Guid Id { get; set; }
    public List<AttachmentModel> Attachments { get; set; }
    public SendStatus Status { get; set; }
    public MessageFormat Format { get; set; }
    public DateTimeOffset Created { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public DateTimeOffset PurgeAfter { get; set; }
    public MessageTarget Target { get; set; }
    public string SenderId { get; set; }
}
