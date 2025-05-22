namespace Webefinity.Module.Messaging.Data;

public class Attachment
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; } = 0;
    public Guid AttachmentId { get; set; } = default!;
    public virtual Message Message { get; set; } = default!;
}
