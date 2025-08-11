using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.ContentRoot.IndexUI.Interfaces;
using Webefinity.Results;

namespace Webefinity.ContentRoot.Index.Services;

public class FileBrowserService : IFileBrowserService
{
    private readonly IServiceProvider serviceProvider;

    public FileBrowserService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    private IIndexedContentRootLibrary GetIndexedContentRoot(string? key)
    {
        return key == null
            ? serviceProvider.GetRequiredService<IIndexedContentRootLibrary>()
            : serviceProvider.GetRequiredKeyedService<IIndexedContentRootLibrary>(key);
    }


    public async Task<Result> DeleteFileAsync(string collection, string filenameOrId, string? key)
    {
        var indexedContentRoot = GetIndexedContentRoot(key);
        var collectionDef = new CollectionDef(collection);
        return await indexedContentRoot.DeleteFileAsync(collectionDef, filenameOrId);
    }

    public async Task<ValueResult<FileMetadataModel>> GetFileMetadataAsync(string collection, string filenameOrId, string? key)
    {
        var indexedContentRoot = GetIndexedContentRoot(key);
        var collectionDef = new CollectionDef(collection);
        return await indexedContentRoot.GetFileMetadataAsync(collectionDef, filenameOrId);
    }

    public async Task<PaginatedContainer<FileMetadataModel>> ListFiles(string collection, string? search = null, int skip = 0, int take = 10, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(key);
        return await indexedContentRoot.ListAsync(new CollectionDef(collection), search, skip, take);
    }

    public async Task<ValueResult<Stream>> LoadFileContentAsync(string collection, string filenameOrId, string? key)
    {
        var indexedContentRoot = GetIndexedContentRoot(key);
        var collectionDef = new CollectionDef(collection);
        var fileExists = await indexedContentRoot.FileExistsAsync(collectionDef, filenameOrId);
        if (fileExists?.Value?.Exists ?? false)
        {
            var tuple = await indexedContentRoot.LoadReadStreamAsync(collectionDef, filenameOrId);
            var readStream = tuple.Item1;
            var metadataModel = tuple.Item2;

            return ValueResult<Stream>.Ok(readStream);
        }
        else
        {
            return ValueResult<Stream>.Fail("File not found.", ResultReasonType.NotFound);
        }
    }

    public async Task<Result> UploadFileAsync(string collection, string filename, string contentType, Stream content, string? key)
    {
        var indexedContentRoot = GetIndexedContentRoot(key);
        var tempFilename = Path.GetTempFileName();
        try
        {
            using (var fileStream = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await content.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                List<MetadataModel> metadataModels = [MetadataModel.Build("Size-Bytes", new ContentSizeMetadataModel(fileStream.Length))];
                fileStream.Seek(0, SeekOrigin.Begin);
                await indexedContentRoot.SaveAsync(new CollectionDef(collection), filename, fileStream, contentType, true, metadataModels);
                fileStream.Close();
            }
        }
        finally
        {
            if (File.Exists(tempFilename))
                File.Delete(tempFilename);
        }

        return Result.Ok();
    }
}
