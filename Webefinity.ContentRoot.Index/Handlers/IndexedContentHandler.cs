using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.ContentRoot.Index.Services;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Webefinity.ContentRoot.Index.Handlers;

public static class IndexedContentHandler
{
    private static IIndexedContentRootLibrary GetIndexedContentRoot(IServiceProvider sp, string? key)
    {
        return key == null
            ? sp.GetRequiredService<IIndexedContentRootLibrary>()
            : sp.GetRequiredKeyedService<IIndexedContentRootLibrary>(key);
    }

    public static async Task<IResult> GetList(
        IServiceProvider sp, string collection, string? search, int skip, int take, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var result = await indexedContentRoot.ListAsync(new CollectionDef(collection), search, skip, take);
        return HttpResults.Ok(result);
    }

    public static async Task<IResult> PostFile(
        IServiceProvider sp, string collection, Stream content,
        string contentType, string fileName, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var tempFilename = Path.GetTempFileName();
        try
        {
            using (var fileStream = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                await content.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
                List<MetadataModel> metadataModels = [MetadataModel.Build("Size-Bytes", new ContentSizeMetadataModel(fileStream.Length))];
                fileStream.Seek(0, SeekOrigin.Begin);
                await indexedContentRoot.SaveAsync(new CollectionDef(collection), fileName, fileStream, contentType, true, metadataModels);
                fileStream.Close();
            }
        }
        finally
        {
            if (File.Exists(tempFilename))
                File.Delete(tempFilename);
        }
        return HttpResults.Ok();
    }

    public static async Task<IResult> GetFile(
        IServiceProvider sp, HttpResponse response, string collection, string filename, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var collectionDef = new CollectionDef(collection);
        var fileExists = await indexedContentRoot.FileExistsAsync(collectionDef, filename);
        if (fileExists.Value)
        {
            var tuple = await indexedContentRoot.LoadReadStreamAsync(collectionDef, filename);
            var readStream = tuple.Item1;
            var metadataModel = tuple.Item2;
            var contentType = metadataModel.GetMetadataValue<string>("Content-Type") ?? "application/octet-stream";

            return HttpResults.Stream(readStream, contentType, filename);
        }
        else
        {
            return HttpResults.NotFound();
        }
    }
    public static async Task<IResult> GetFileMeta(
        IServiceProvider sp, HttpResponse response, string collection, string filename, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var collectionDef = new CollectionDef(collection);
        var file = await indexedContentRoot.GetFileMetadataAsync(collectionDef, filename);
        return HttpResults.Json(file);
    }

    public static async Task<IResult> DeleteFile(
        IServiceProvider sp, string collection, string filename, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var collectionDef = new CollectionDef(collection);
        var result = await indexedContentRoot.DeleteFileAsync(collectionDef, filename);
        if (result.Success)
        {
            return HttpResults.Ok();
        }
        if (result.Reason == Results.ResultReasonType.NotFound)
        {
            return HttpResults.NotFound(result);
        }
        return HttpResults.Problem(result.ToString());
    }
}
