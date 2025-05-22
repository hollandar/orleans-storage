namespace Webefinity.ContentRoot.Index.Data;

public class FileMetadata
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Key { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public ICollection<Metadata> Metadata { get; set; } = new List<Metadata>();
}
