
namespace Webefinity.Module.Messaging.Abstractions.Models;

public class MessageListModel
{
    public string SenderId { get; set; } = string.Empty;
    public MessageTarget Target { get; set; }
    public SendStatus Status { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public Guid Id { get; set; }
    public string To { get; set; } = string.Empty;
}
