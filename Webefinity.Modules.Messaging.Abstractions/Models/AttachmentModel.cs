
namespace Webefinity.Module.Messaging.Abstractions.Models;

public class AttachmentModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ContentType { get; set; }
    public long Length { get; set; }
}
