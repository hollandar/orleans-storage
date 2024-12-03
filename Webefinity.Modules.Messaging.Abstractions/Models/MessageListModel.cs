
namespace Webefinity.Module.Messaging.Abstractions.Models;

public class MessageListModel
{
    public string SenderId { get; set; }
    public MessageTarget Target { get; set; }
    public SendStatus Status { get; set; }
    public DateTimeOffset? Sent { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Content { get; set; }
    public string? Subject { get; set; }
    public Guid Id { get; set; }
    public string To { get; set; }
}
