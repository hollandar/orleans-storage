
namespace Webefinity.Module.Messaging.Abstractions.Models;

public class AttachmentModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
}
