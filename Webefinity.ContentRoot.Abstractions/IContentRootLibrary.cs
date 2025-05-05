namespace Webefinity.ContentRoot.Abstractions;

public interface IContentRootLibrary
{
    public Task<StreamReader> LoadReaderAsync(CollectionDef collection, string file);
    public Stream LoadReadStream(CollectionDef collection, string file);
    public StreamReader LoadReader(CollectionDef collection, string file);
    public Task<Stream> LoadReadStreamAsync(CollectionDef collection, string file);
    public string Load(CollectionDef collection, string file);
    public Task<string> LoadAsync(CollectionDef collection, string file);
    public IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null);
    public IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null);
    public bool FileExists(CollectionDef collection, string file);
    public Task<bool> FileExistsAsync(CollectionDef collection, string file);
    public bool DirectoryExists(CollectionDef collection, string directory);
    public Task<bool> DirectoryExistsAsync(CollectionDef collection, string directory);
    public Task<T> LoadJsonAsync<T>(CollectionDef collection, string file);
    public T LoadJson<T>(CollectionDef collection, string file);
    public Task SaveAsync(CollectionDef collection, string file, Stream content, string? contentType = null);
    public Task RemoveAsync(CollectionDef collection, string file);
}
