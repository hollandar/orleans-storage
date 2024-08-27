namespace Webefinity.ContentRoot;

public enum ContentRootType { File }
public class ContentRootOptions
{
    public ContentRootType Type { get; set; } = ContentRootType.File;
    public string? Path { get; set; } = "./contentRoot/";

}
