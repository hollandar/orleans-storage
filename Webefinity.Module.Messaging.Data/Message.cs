namespace Webefinity.Module.Messaging.Data;
public class Message
{
    public Guid Id { get; set; } = UUIDNext.Uuid.NewDatabaseFriendly(UUIDNext.Database.PostgreSql);
    public string SenderId { get; set; } = string.Empty;
    public MessageTarget Target { get; set; } = MessageTarget.None;
    public string? Subject { get; set; } = null;
    public string Content { get; set; } = string.Empty;
    public virtual List<Attachment> Attachments { get; set; } = new List<Attachment>();
    public MessageFormat Format { get; set; } = MessageFormat.None;
    public virtual List<Address> Addresses { get; set; } = new List<Address>();
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Sent { get; set; } = null;
    public SendStatus Status { get; set; } = SendStatus.None;
    public string? Error { get; set; } = null;
    public DateTimeOffset PurgeAfter { get; set; } = DateTimeOffset.UtcNow.AddDays(365);
}