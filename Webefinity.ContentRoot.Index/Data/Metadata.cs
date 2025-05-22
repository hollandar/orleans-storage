using System.Text.Json;

namespace Webefinity.ContentRoot.Index.Data;

public class Metadata: IDisposable
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid FileMetadataId { get; set; } = Guid.Empty;
    public virtual FileMetadata? FileMetadata { get; set; } = null;
    public string Key { get; set; } = string.Empty;
    public JsonDocument Value { get; set; } = JsonDocument.Parse("{}");

    public void Dispose() => Value?.Dispose();
}
