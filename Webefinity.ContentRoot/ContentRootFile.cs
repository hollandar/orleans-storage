using GlobExpressions;
using Microsoft.Extensions.Options;

namespace Webefinity.ContentRoot;

public class ContentRootFile : IContentRootLibrary
{
    private readonly IOptions<ContentRootOptions> options;
    private string contentRootPath;

    public ContentRootFile(IOptions<ContentRootOptions> options)
    {
        this.options = options;
        if (!Directory.Exists(options.Value.Path))
            throw new Exception("Content root does not exist.");

        contentRootPath = Path.Combine(options.Value.Path);
    }

    private string ToFile(CollectionDef collection, string file) => collection.ToFile(file);

    public string Load(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, file);
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return File.ReadAllText(path);
    }

    public async Task<string> LoadAsync(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, file);
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return await File.ReadAllTextAsync(path);
    }

    public StreamReader LoadReader(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, file);
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return new StreamReader(path, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read });
    }
    
    public Stream LoadReadStream(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, file);
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public async IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        var relative = Path.Combine(this.contentRootPath, collection.Collection);
        var path = insidePath is not null ? Path.Combine(contentRootPath, collection.Collection, insidePath) : Path.Combine(contentRootPath, collection.Collection);
        if (!Directory.Exists(path))
            throw new LibraryPathNotFoundException("Library path was not found.", path);

        var files = Directory.GetFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = true });
        foreach (var file in files)
        {
            if (Glob.IsMatch(file, glob))
            {
                yield return Path.GetRelativePath(relative, file);
            }
        }
    }

    public async IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        var relative = Path.Combine(this.contentRootPath, collection.Collection);
        var path = insidePath is not null ? Path.Combine(contentRootPath, collection.Collection, insidePath) : Path.Combine(contentRootPath, collection.Collection);
        if (!Directory.Exists(path))
            throw new LibraryPathNotFoundException("Library path was not found.", path);

        var files = Directory.GetFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = false });
        foreach (var file in files)
        {
            if (Glob.IsMatch(file, glob))
            {
                yield return Path.GetRelativePath(relative, file);
            }
        }
    }

    public bool FileExists(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, file);
        return File.Exists(path);
    }

    public bool DirectoryExists(CollectionDef collection, string directory)
    {
        var path = Path.Combine(contentRootPath, collection.Collection, directory);
        return Directory.Exists(path);
    }
}
