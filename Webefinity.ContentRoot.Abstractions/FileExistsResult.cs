namespace Webefinity.ContentRoot.Abstractions;

public class FileExistsResult
{
    public FileExistsResult(bool exists = false, string? etag = null, string? contentType = null) { 
        this.Exists = exists;
        this.ETag = etag;
        this.ContentType = contentType;
    }
    public bool Exists { get; set; } = false;
    public string? ETag { get; set; }
    public string? ContentType { get; set; }

    public static implicit operator bool(FileExistsResult result) => result.Exists;
    public static FileExistsResult False => new FileExistsResult(false);
}
