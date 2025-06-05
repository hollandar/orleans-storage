using GlobExpressions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System.Text.Json;
using System.Text.RegularExpressions;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.Extensions;

namespace Webefinity.ContentRoot.S3;

public partial class ContentRootS3 : IContentRootLibrary
{
    private readonly IMinioClient minioClient;
    private readonly IOptions<ContentRootOptions> options;
    private readonly IContentPathBuilder pathBuilder;
    private readonly string bucket;
    private readonly string? path = null;

    public ContentRootS3(IServiceProvider serviceProvider, IOptions<ContentRootOptions> objectStoreOptions, IContentPathBuilder pathBuilder)
    {
        if (objectStoreOptions.Value.Type != ContentRootType.S3)
        {
            throw new InvalidOperationException("ContentRootOptions.Type is not S3");
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<string>("Bucket", out var bucket) || bucket is null)
        {
            throw new InvalidOperationException("ContentRootOptions.Bucket not set, should be a simple root bucket name.");
        }
        else
        {
            this.bucket = bucket;
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<string>("Endpoint", out var endpoint) || endpoint is null)
        {
            throw new InvalidOperationException("ContentRootOptions.Endpoint not set, should be the endpoint URL.");
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<string>("AccessKey", out var accessKey) || accessKey is null)
        {
            throw new InvalidOperationException("ContentRootOptions.AccessKey not set, should be the access key.");
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<string>("SecretKey", out var secretKey) || secretKey is null)
        {
            throw new InvalidOperationException("ContentRootOptions.SecretKey not set, should be the secret key.");
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<bool>("SSL", out var ssl))
        {
            throw new InvalidOperationException("ContentRootOptions.SSL not set, should be a boolean.");
        }

        if (!objectStoreOptions.Value.Properties.TryGetValue<string>("Region", out var region) || region is null)
        {
            region = "us-east-1";
        }

        if (objectStoreOptions.Value.Properties.TryGetValue<string>("Path", out var path) && path is string)
        {
            this.path = (string)path;
        }

        this.minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, (string)secretKey)
            .WithRegion(region)
            .WithSSL(ssl)
            .Build();

        this.options = objectStoreOptions;
        this.pathBuilder = pathBuilder;
    }

    [GeneratedRegex("^(?:(?<directoryName>[^\\/]+)[\\/]{0,1})+$")]
    private partial Regex DirectoryNameRegex();

    private string BuildPath(CollectionDef collection, string? folder) => this.pathBuilder.GetPath(collection, folder, this.path);
    

    public bool DirectoryExists(CollectionDef collection, string directory) => throw new NotImplementedException("Only the async interface is supported.");
    public async Task<bool> DirectoryExistsAsync(CollectionDef collection, string directory)
    {
        try
        {
            if (!DirectoryNameRegex().IsMatch(directory))
            {
                throw new ArgumentException("Directory is not in the correct format.  Should be '/' separated directory names.");
            }

            var statObjectArgs = new StatObjectArgs().WithBucket(this.bucket).WithObject(BuildPath(collection, directory));

            await this.minioClient.StatObjectAsync(statObjectArgs);
        }
        catch (MinioException e)
        {
            if (e is ObjectNotFoundException)
            {
                return false;
            }
            throw;
        }

        return true;
    }

    private async IAsyncEnumerable<string> EnumerateInternalAsync(CollectionDef collection, string glob, bool recursive, string? insidePath = null)
    {
        if (!DirectoryNameRegex().IsMatch(insidePath))
        {
            throw new ArgumentException("Directory is not in the correct format.  Should be '/' separated directory names.");
        }

        var args = new ListObjectsArgs()
            .WithBucket(this.bucket)
            .WithRecursive(recursive)
            .WithPrefix(BuildPath(collection, insidePath));

        await foreach (var item in this.minioClient.ListObjectsEnumAsync(args))
        {
            if (Glob.IsMatch(item.Key, glob))
            {
                yield return item.Key;
            }
        }
    }

    public IAsyncEnumerable<string> EnumerateAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        return EnumerateInternalAsync(collection, glob, false, insidePath);
    }

    public IAsyncEnumerable<string> EnumerateRecursiveAsync(CollectionDef collection, string glob, string? insidePath = null)
    {
        return EnumerateInternalAsync(collection, glob, true, insidePath);
    }

    public async Task<FileExistsResult> FileExistsAsync(CollectionDef collection, string file)
    {
        try
        {
            if (!DirectoryNameRegex().IsMatch(file))
            {
                throw new ArgumentException("File is not in the correct format.  Should be '/' separated directory/file names.");
            }

            var statObjectArgs = new StatObjectArgs().WithBucket(this.bucket).WithObject(BuildPath(collection, file));

            var objectStat = await this.minioClient.StatObjectAsync(statObjectArgs);
            return new FileExistsResult(true, objectStat.ETag);
        }
        catch (MinioException e)
        {
            if (e is ObjectNotFoundException)
            {
                return FileExistsResult.False;
            }
            throw;
        }
    }

    public FileExistsResult FileExists(CollectionDef collection, string file) => throw new NotImplementedException("Only the async interface is supported.");

    public string Load(CollectionDef collection, string file) => throw new NotImplementedException("Only the async interface is supported.");

    public async Task<string> LoadAsync(CollectionDef collection, string file)
    {
        using var streamReader = await LoadReaderAsync(collection, file);
        return await streamReader.ReadToEndAsync();
    }

    public StreamReader LoadReader(CollectionDef collection, string file) => throw new NotImplementedException("Only the async interface is supported.");
    public async Task<StreamReader> LoadReaderAsync(CollectionDef collection, string file)
    {
        var stream = await LoadReadStreamAsync(collection, file);
        return new StreamReader(stream);
    }

    public Stream LoadReadStream(CollectionDef collection, string file) => throw new NotImplementedException("Only the async interface is supported.");
    public async Task<Stream> LoadReadStreamAsync(CollectionDef collection, string file)
    {
        try
        {
            if (!DirectoryNameRegex().IsMatch(file))
            {
                throw new ArgumentException("File is not in the correct format.  Should be '/' separated directory/file names.");
            }

            MemoryStream contentStream = new();
            var path = BuildPath(collection, file);
            var args = new GetObjectArgs().WithBucket(this.bucket)
                .WithObject(BuildPath(collection, file))
                .WithCallbackStream((stream) => { 
                    stream.CopyTo(contentStream); 
                });

            var stat = await minioClient.GetObjectAsync(args);

            contentStream.Seek(0, SeekOrigin.Begin);

            return contentStream;
        }
        catch (MinioException e)
        {
            if (e is ObjectNotFoundException)
            {
                throw new LibraryPathNotFoundException("Path not found.", e, file);
            }

            throw;
        }
    }

    public T LoadJson<T>(CollectionDef collection, string file) => throw new NotImplementedException("Only the async interface is supported.");
    public async Task<T> LoadJsonAsync<T>(CollectionDef collection, string file)
    {
        var content = await LoadAsync(collection, file);
        var value = JsonSerializer.Deserialize<T>(content);
        if (value is null)
        {
            throw new Exception("Failed to deserialize JSON content.");
        }

        return value;
    }

    public async Task SaveAsync(CollectionDef collection, string file, Stream content, string? contentType = null)
    {
        try
        {
            if (!DirectoryNameRegex().IsMatch(file))
            {
                throw new ArgumentException("File is not in the correct format.  Should be '/' separated directory/file names.");
            }

            MemoryStream contentStream = new();
            var path = BuildPath(collection, file);
            var args = new PutObjectArgs().WithBucket(this.bucket)
                .WithObject(BuildPath(collection, file))
                .WithStreamData(content)
                .WithObjectSize(content.Length)
                .WithContentType(contentType ?? "application/octet-stream");

            await minioClient.PutObjectAsync(args);
        }
        catch (MinioException e)
        {
            if (e is ObjectNotFoundException)
            {
                throw new LibraryPathNotFoundException("Path not found.", e, file);
            }

            throw;
        }
    }

    public async Task RemoveAsync(CollectionDef collection, string file)
    {
        try
        {
            if (!DirectoryNameRegex().IsMatch(file))
            {
                throw new ArgumentException("File is not in the correct format.  Should be '/' separated directory/file names.");
            }

            var args = new RemoveObjectArgs().WithBucket(this.bucket)
                .WithObject(BuildPath(collection, file));
            await minioClient.RemoveObjectAsync(args);
        }
        catch (MinioException e)
        {
            if (e is ObjectNotFoundException)
            {
                throw new LibraryPathNotFoundException("Path not found.", e, file);
            }
            throw;
        }
    }
}
