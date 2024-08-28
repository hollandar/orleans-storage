namespace Webefinity.ContentRoot;

public interface IContentRootLibrary
{
    public StreamReader LoadReader(CollectionDef collection, string file);
    public Stream LoadReadStream(CollectionDef collection, string file);
    public string Load(CollectionDef collection, string file);
    public Task<string> LoadAsync(CollectionDef collection, string file);
    public IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null);
    public IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null);
    public bool FileExists(CollectionDef collection, string file);
    public bool DirectoryExists(CollectionDef collection, string directory);
}
