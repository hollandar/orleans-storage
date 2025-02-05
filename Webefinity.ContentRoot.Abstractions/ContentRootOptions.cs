namespace Webefinity.ContentRoot.Abstractions;

public enum ContentRootType { None, File, S3 }
public class ContentRootOptions
{
    public ContentRootType Type { get; set; } = ContentRootType.None;
    public Dictionary<string, object?> Properties { get; set; } = new();

}

public class ContentRootOptionsBase : ContentRootOptions
{
    public Dictionary<string, ContentRootOptions> Options { get; set; } = new();
}
