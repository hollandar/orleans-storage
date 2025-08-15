using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.ContentRoot.Index.Services;
using Webefinity.ContentRoot.Index.Shared;
using HttpResults = Microsoft.AspNetCore.Http.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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
        IServiceProvider sp, HttpRequest request, HttpResponse response, string collection, string filename, string? key = null)
    {
        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var collectionDef = new CollectionDef(collection);
        var fileExists = await indexedContentRoot.FileExistsAsync(collectionDef, filename);
        if (fileExists.Value?.Exists ?? false)
        {
            if (request.Headers.ContainsKey("If-None-Match"))
            {
                var oldEtag = request.Headers["If-None-Match"].First();
                if (fileExists.Value.ETag is not null && oldEtag == fileExists.Value.ETag)
                {
                    return HttpResults.StatusCode(304);
                }
            }

            var tuple = await indexedContentRoot.LoadReadStreamAsync(collectionDef, filename);
            var readStream = tuple.Item1;
            var metadataModel = tuple.Item2;
            var contentType = metadataModel.GetMetadataValue<string>("Content-Type") ?? "application/octet-stream";
            if (fileExists.Value.ETag is not null)
            {
                response.Headers["ETag"] = fileExists.Value.ETag;
            }
            response.Headers["Cache-Control"] = "public, max-age=3600"; //  1 hour
            return HttpResults.Stream(readStream, contentType, filename);
        }
        else
        {
            return HttpResults.NotFound();
        }
    }

    public static async Task<IResult> GetSizedFile(
       IServiceProvider sp, HttpRequest request, HttpResponse response, string collection, string imageSize, string filename, string? key = null)
    {
        if (!Enum.TryParse<ImageSizeEnum>(imageSize, ignoreCase: true, out var imageSizeEnum))
        {
            return HttpResults.BadRequest($"Invalid image size: {imageSize}");
        }

        var indexedContentRoot = GetIndexedContentRoot(sp, key);
        var collectionDef = new CollectionDef(collection);
        var fileExists = await indexedContentRoot.FileExistsAsync(collectionDef, filename);
        if (fileExists.Value?.Exists ?? false)
        {
            if (request.Headers.ContainsKey("If-None-Match"))
            {
                var oldEtag = request.Headers["If-None-Match"].First();
                if (fileExists.Value.ETag is not null && oldEtag == fileExists.Value.ETag)
                {
                    return HttpResults.StatusCode(304);
                }
            }

            var tuple = await indexedContentRoot.LoadReadStreamAsync(collectionDef, filename);
            var readStream = tuple.Item1;
            var metadataModel = tuple.Item2;
            var contentType = metadataModel.GetMetadataValue<string>("Content-Type") ?? "application/octet-stream";
            if (fileExists.Value.ETag is not null)
            {
                response.Headers["ETag"] = fileExists.Value.ETag;
            }
            response.Headers["Cache-Control"] = "public, max-age=3600"; //  1 hour

            var webpFileName = $"{filename}.{imageSize}.webp";
            var resizedFileMetadata = await indexedContentRoot.GetFileMetadataAsync(collectionDef, webpFileName);
            if (resizedFileMetadata.Success && (resizedFileMetadata.Value?.GetMetadataValue<bool?>("Is-Derived") ?? false) && resizedFileMetadata.Value?.GetMetadataValue<string>("Original-File-ETag") == fileExists.Value.ETag)
            {
                // If the resized image is derived from the original and has the same ETag, return it
                var cachedResizedStream = await indexedContentRoot.LoadReadStreamAsync(collectionDef, webpFileName);
                return HttpResults.Stream(cachedResizedStream.Item1, "image/webp", webpFileName);
            }

            var image = Image.Load(readStream);
            switch (imageSizeEnum)
            {
                case ImageSizeEnum.Small:
                    image.Mutate(x => x.Resize(1024, 0));
                    break;
                case ImageSizeEnum.Medium:
                    image.Mutate(x => x.Resize(1920, 0));
                    break;
                case ImageSizeEnum.Large:
                    image.Mutate(x => x.Resize(2560, 0));
                    break;
                case ImageSizeEnum.ExtraLarge:
                    image.Mutate(x => x.Resize(3840, 0));
                    break;
                case ImageSizeEnum.Thumbnail:
                    image.Mutate(x => x.Resize(150, 0));
                    break;
                case ImageSizeEnum.Icon:
                    image.Mutate(x => x.Resize(32, 32));
                    break;
                case ImageSizeEnum.Custom:
                    // Custom logic can be added here
                    break;
            }

            var resizedStream = new MemoryStream();
            image.Save(resizedStream, new SixLabors.ImageSharp.Formats.Webp.WebpEncoder());

            resizedStream.Seek(0, SeekOrigin.Begin);
            await indexedContentRoot.SaveAsync(collectionDef, webpFileName, resizedStream, "image/webp", true, new List<MetadataModel>
            {
                MetadataModel.Build<string>("Content-Type", "image/webp"),
                MetadataModel.Build<ImageSizeEnum>("Image-Size", imageSizeEnum),
                MetadataModel.Build<string>("Original-File-ETag", fileExists.Value.ETag ?? string.Empty),
                MetadataModel.Build<bool>("Is-Derived", true)

            });

            resizedStream.Seek(0, SeekOrigin.Begin);
            return HttpResults.Stream(resizedStream, "image/webp", webpFileName);
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
