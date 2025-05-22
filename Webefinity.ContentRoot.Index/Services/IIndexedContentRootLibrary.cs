using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.Results;

namespace Webefinity.ContentRoot.Index.Services;

public interface IIndexedContentRootLibrary
{
    Task<Result> DeleteFileAsync(CollectionDef collection, string file);
    Task<ValueResult<bool>> FileExistsAsync(CollectionDef collection, string file);
    Task<ValueResult<FileMetadataModel>> GetFileMetadataAsync(CollectionDef collection, string fileOrId);
    Task<PaginatedContainer<FileMetadataModel>> ListAsync(CollectionDef collection, string? search = null, int skip = 0, int take = 10);
    Task<(Stream, FileMetadataModel)> LoadReadStreamAsync(CollectionDef collection, string file);
    Task SaveAsync(CollectionDef collection, string file, Stream content, string contentType = "application/octet-stream", bool replaceMetadata = false, params IList<MetadataModel> metadatas);
}
