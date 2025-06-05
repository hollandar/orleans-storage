namespace Webefinity.ContentRoot.Abstractions;

public class FileExistsResult
{
    public FileExistsResult(bool exists = false, string? etag = null) { 
        this.Exists = exists;
        this.ETag = etag;
    }
    public bool Exists { get; set; } = false;
    public string? ETag { get; set; }

    public static implicit operator bool(FileExistsResult result) => result.Exists;
    public static FileExistsResult False => new FileExistsResult(false);
}
