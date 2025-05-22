using GlobExpressions;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.Extensions;

namespace Webefinity.ContentRoot;

public class ContentRootFile : IContentRootLibrary
{
    private readonly IOptions<ContentRootOptions> options;
    private readonly IContentPathBuilder pathBuilder;
    private string contentRootPath;

    public ContentRootFile(IServiceProvider serviceProvider, IOptions<ContentRootOptions> options, IContentPathBuilder pathBuilder)
    {
        this.options = options;
        this.pathBuilder = pathBuilder;
        if (options.Value.Properties.TryGetValue<string>("Path", out var optionsPath))
        {
            if (!Directory.Exists(optionsPath))
                throw new Exception("Content root does not exist.");

            contentRootPath = Path.Combine(optionsPath);
        }
        else
        {
            throw new ArgumentException("Content root path is not set. Set the Path property in configuration");
        }
    }

    public string Load(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, folder: file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return File.ReadAllText(path);
    }

    public async Task<string> LoadAsync(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return await File.ReadAllTextAsync(path);
    }

    public StreamReader LoadReader(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return new StreamReader(path, new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read });
    }

    public Task<StreamReader> LoadReaderAsync(CollectionDef collection, string file)
    {
        return Task.FromResult(LoadReader(collection, file));
    }


    public Stream LoadReadStream(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);

        return new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public Task<Stream> LoadReadStreamAsync(CollectionDef collection, string file)
    {
        return Task.FromResult(LoadReadStream(collection, file));
    }

#pragma warning disable CS1998
    public async IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, insidePath));
        if (!Directory.Exists(path))
            throw new LibraryPathNotFoundException("Library path was not found.", path);

        var files = Directory.GetFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = true });
        foreach (var file in files)
        {
            if (Glob.IsMatch(file, glob))
            {
                yield return Path.GetRelativePath(path, file);
            }
        }

        await Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, insidePath));
        if (!Directory.Exists(path))
            throw new LibraryPathNotFoundException("Library path was not found.", path);

        var files = Directory.GetFiles(path, "*", new EnumerationOptions { RecurseSubdirectories = false });
        foreach (var file in files)
        {
            if (Glob.IsMatch(file, glob))
            {
                yield return Path.GetRelativePath(path, file);
            }
        }
    }
#pragma warning restore CS1998

    public bool FileExists(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        return File.Exists(path);
    }

    public Task<bool> FileExistsAsync(CollectionDef collection, string file)
    {
        return Task.FromResult(FileExists(collection, file));
    }

    public bool DirectoryExists(CollectionDef collection, string directory)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, directory));
        return Directory.Exists(path);
    }

    public Task<bool> DirectoryExistsAsync(CollectionDef collection, string directory) => Task.FromResult(DirectoryExists(collection, directory));


    public T LoadJson<T>(CollectionDef collection, string file) {
        var content = Load(collection, file);
        var value = JsonSerializer.Deserialize<T>(content);
        if (value is null)
        {
            throw new Exception("Failed to deserialize JSON content.");
        }

        return value;
    }

    public Task<T> LoadJsonAsync<T>(CollectionDef collection, string file)
    {
        return Task.FromResult(LoadJson<T>(collection, file));
    }

    public Task SaveAsync(CollectionDef collection, string file, Stream content, string? contentType = null)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);


        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        content.CopyTo(fileStream);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(CollectionDef collection, string file)
    {
        var path = Path.Combine(contentRootPath, this.pathBuilder.GetPath(collection, file));
        if (!File.Exists(path))
            throw new LibraryPathNotFoundException("File was not found in LoadReader.", path);


        File.Delete(path);

        return Task.CompletedTask;
    }
}
