using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Data;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.Results;

namespace Webefinity.ContentRoot.Index.Services;

public class IndexedContentRootService : IIndexedContentRootLibrary
{
    private readonly IContentRootLibrary contentRootLibrary;
    private readonly IFileMetadataDbContext fileMetadataDbContext;
    private readonly string key;

    public IndexedContentRootService(IContentRootLibrary contentRootLibrary, IFileMetadataDbContext fileMetadataDbContext, string key = "Default")
    {
        ArgumentNullException.ThrowIfNull(contentRootLibrary, nameof(contentRootLibrary));
        this.contentRootLibrary = contentRootLibrary;
        this.fileMetadataDbContext = fileMetadataDbContext;
        this.key = key;
    }

    public async Task<ValueResult<bool>> FileExistsAsync(CollectionDef collection, string file)
    {
        var fileLower = file.ToLower();
        var fileMetaExists = fileMetadataDbContext.Files.Where(r => r.Collection == collection.Collection && r.Key == key && r.FileName.ToLower() == fileLower).Any();
        var contentExists = await this.contentRootLibrary.FileExistsAsync(collection, file);

        return ValueResult<bool>.Ok(fileMetaExists && contentExists);
    }

    public Task<ValueResult<FileMetadataModel>> GetFileMetadataAsync(CollectionDef collection, string fileOrId)
    {
        FileMetadata? fileMetadata = null;

        if (Guid.TryParse(fileOrId, out var id))
        {
            fileMetadata = fileMetadataDbContext.Files
                .Include("Metadata")
                .Where(r => r.Collection == collection.Collection && r.Key == key && r.Id == id).SingleOrDefault();
        }
        else
        {
            var fileLower = fileOrId.ToLower();
            fileMetadata = fileMetadataDbContext.Files
                .Include("Metadata")
                .Where(r => r.Collection == collection.Collection && r.Key == key && r.FileName.ToLower() == fileLower).SingleOrDefault();
        }

        if (fileMetadata is null)
        {
            return Task.FromResult(ValueResult<FileMetadataModel>.Fail($"File metadata not found for {fileOrId}.", ResultReasonType.NotFound));
        }

        return Task.FromResult(ValueResult<FileMetadataModel>.Ok(fileMetadata.ToFileMetadataModel()));
    }

    public async Task<(Stream, FileMetadataModel)> LoadReadStreamAsync(CollectionDef collection, string file)
    {
        var fileMetadata = fileMetadataDbContext.Files
            .Include("Metadata")
            .Where(r => r.Key == this.key && r.Collection == collection.Collection && r.FileName == file).FirstOrDefault();
        if (fileMetadata is null)
        {
            throw new FileNotFoundException($"File {file} not found in collection {collection.Collection}");
        }

        return (await contentRootLibrary.LoadReadStreamAsync(collection, file), fileMetadata.ToFileMetadataModel());
    }

    public async Task SaveAsync(CollectionDef collection, string file, Stream content, string contentType = "application/octet-stream", bool replaceMetadata = false, params IList<MetadataModel> metadatas)
    {
        var contentTypeMetadata = metadatas.FirstOrDefault(r => r.Key == "Content-Type");
        if (contentTypeMetadata is not null)
        {
            metadatas.Remove(contentTypeMetadata);
        }

        metadatas.Add(new MetadataModel { Key = "Content-Type", Value = JsonDocument.Parse($"\"{contentType}\"") });

        await this.contentRootLibrary.SaveAsync(collection, file, content);
        await this.SetMetadataAsync(collection, file, replaceMetadata, metadatas);

    }

    public Task<PaginatedContainer<FileMetadataModel>> ListAsync(CollectionDef collection, string? search = null, int skip = 0, int take = 10)
    {
        var files = this.fileMetadataDbContext.Files.Include("Metadata")
                        .Where(r => r.Key == this.key && r.Collection == collection.Collection)
                        .OrderBy(r => r.FileName).AsQueryable();

        if (!String.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            files = files.Where(r => r.FileName.ToLower().Contains(searchLower));
        }

        var totalCount = files.Count();
        var paginatedFiles = files
                        .Skip(skip).Take(take)
            .Select(r => r.ToFileMetadataModel());

        return Task.FromResult(new PaginatedContainer<FileMetadataModel>(paginatedFiles, totalCount));
    }

    public IEnumerable<string> FindByCollection(CollectionDef collectionDef)
    {
        var files = this.fileMetadataDbContext.Files
            .Where(r => r.Key == this.key && r.Collection == collectionDef.Collection)
            .Select(r => r.FileName);
        return files.AsEnumerable();
    }

    public IEnumerable<string> FindByMetadata(CollectionDef collection, Func<JsonDocument, bool> filter)
    {
        var files = this.fileMetadataDbContext.Metadata
            .Where(r => r.FileMetadata != null && r.FileMetadata.Key == this.key && r.FileMetadata.Collection == collection.Collection)
            .Where(r => filter(r.Value))
            .Select(r => r.FileMetadata!.FileName);

        return files.AsEnumerable();
    }

    public async Task SetMetadataAsync(CollectionDef collection, string file, bool replaceMetadatas = false, params IList<MetadataModel> metadatas)
    {
        var fileMetadata = this.fileMetadataDbContext.Files.Where(r => r.Key == this.key && r.Collection == collection.Collection && r.FileName == file)
            .FirstOrDefault();

        // Create a metadata if there is not one
        if (fileMetadata is null)
        {
            fileMetadata = new FileMetadata
            {
                Id = Guid.CreateVersion7(),
                Key = this.key,
                Collection = collection.Collection,
                FileName = file,
            };
            this.fileMetadataDbContext.Files.Add(fileMetadata);
        }

        // Update or create metadata values
        var existingMetadatas = this.fileMetadataDbContext.Metadata.Where(r => r.FileMetadataId == fileMetadata.Id).ToDictionary(r => r.Key, r => r);
        foreach (var metadata in metadatas)
        {
            Metadata replacementMetadata;
            if (existingMetadatas.ContainsKey(metadata.Key))
            {
                replacementMetadata = existingMetadatas[metadata.Key];
            }
            else
            {
                replacementMetadata = new Metadata
                {
                    Id = Guid.CreateVersion7(),
                    FileMetadataId = fileMetadata.Id,
                    Key = metadata.Key,
                };
                this.fileMetadataDbContext.Metadata.Add(replacementMetadata);
            }

            replacementMetadata.Value = metadata.Value;
        }

        // Destroy unused metadata values if replaceMetadata is true    
        if (replaceMetadatas)
        {
            var incommingKeys = metadatas.Select(r => r.Key).ToHashSet();
            foreach (var key in existingMetadatas.Keys)
            {
                if (!incommingKeys.Contains(key))
                {
                    this.fileMetadataDbContext.Metadata.Remove(existingMetadatas[key]);
                }
            }
        }

        await this.fileMetadataDbContext.SaveChangesAsync();

    }

    public TMetadataType? GetMetadata<TMetadataType>(CollectionDef collection, string file, string key)
    {
        var metadata = this.fileMetadataDbContext.Metadata.Where(r => r.FileMetadata != null && r.FileMetadata.Key == this.key && r.FileMetadata.Collection == collection.Collection && r.FileMetadata.FileName == file && r.Key == key).FirstOrDefault();
        if (metadata is null)
        {
            return default(TMetadataType); // Metadata value not found
        }

        var value = metadata.Value;
        return JsonSerializer.Deserialize<TMetadataType>(value);
    }

    public async Task<Result> DeleteFileAsync(CollectionDef collection, string file)
    {
        var fileMetadata = this.fileMetadataDbContext.Files.Where(r => r.Key == this.key && r.Collection == collection.Collection && r.FileName == file).FirstOrDefault();
        if (fileMetadata is not null)
        {
            var fileExists = await this.contentRootLibrary.FileExistsAsync(collection, file);
            if (fileExists)
            {
                await this.contentRootLibrary.RemoveAsync(collection, file);
                this.fileMetadataDbContext.Files.Remove(fileMetadata);
                await this.fileMetadataDbContext.SaveChangesAsync();

                return Result.Ok();
            }
        }

        return Result.Fail("File not found in content root library.", ResultReasonType.NotFound);
    }
}
