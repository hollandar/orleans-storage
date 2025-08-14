using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.ContentRoot.IndexUI.Models;
using Webefinity.Results;

namespace Webefinity.ContentRoot.IndexUI.Interfaces;

public interface IFileBrowserService
{
    Task<PaginatedContainer<FileMetadataModel>> ListFiles(string collection, string? search = null, int skip = 0, int take = 10, string? key = null);
    Task<ValueResult<FileMetadataModel>> GetFileMetadataAsync(string collection, string filenameOrId, string? key);
    Task<Result> DeleteFileAsync(string collection, string filenameOrId, string? key);
    Task<ValueResult<Stream>> LoadFileContentAsync(string collection, string filenameOrId, string? key);
    Task<Result> UploadFileAsync(string collection, string filename, string contentType, Stream content, string? key);
    Task<KeysAndPolicyModel> GetKeysAndPolicyAsync();
}
