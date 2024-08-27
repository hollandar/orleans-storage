namespace Webefinity.ContentRoot;

public record CollectionDef(string Collection)
{
    public string ToFile(string file) => Path.Combine(Collection, file);
}
