using System.Net.Http.Json;
using Webefinity.ContentRoot.Index.Models;
using Webefinity.ContentRoot.IndexUI.Interfaces;
using Webefinity.ContentRoot.IndexUI.Models;
using Webefinity.Results;

namespace Webefinity.ContentRoot.IndexUI.Services;

public class FileBrowserClientService : IFileBrowserService
{
    private readonly HttpClient httpClient;

    public FileBrowserClientService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Result> DeleteFileAsync(string collection, string filenameOrId, string? key)
    {
        var responseMessage = await httpClient.DeleteAsync($"api/icr/{key}/{collection}/{filenameOrId}");
        return responseMessage.IsSuccessStatusCode;
    }

    public async Task<ValueResult<FileMetadataModel>> GetFileMetadataAsync(string collection, string filenameOrId, string? key)
    {
        var responseMessage = await httpClient.GetAsync($"api/icr/{key}/{collection}/meta/{filenameOrId}");
        if (responseMessage.IsSuccessStatusCode)
        {
            // Handle success
            var content = await responseMessage.Content.ReadFromJsonAsync<ValueResult<FileMetadataModel>>();
            ArgumentNullException.ThrowIfNull(content, nameof(content));

            return content;
        }

        throw new Exception($"Unable to query file metadata for {collection}:{filenameOrId}.");
    }

    public async Task<PaginatedContainer<FileMetadataModel>> ListFiles(string collection, string? search = null, int skip = 0, int take = 10, string? key = null)
    {
        var responseMessage = await httpClient.GetAsync($"api/icr/{key}/{collection}/list?skip={skip}&take={take}");
        if (responseMessage.IsSuccessStatusCode)
        {
            // Handle success
            var content = await responseMessage.Content.ReadFromJsonAsync<PaginatedContainer<FileMetadataModel>>();
            if (content is not null)
            {
                return content;
            }
        }
        // Handle failure
        return PaginatedContainer<FileMetadataModel>.Empty;
    }

    public async Task<ValueResult<Stream>> LoadFileContentAsync(string collection, string filenameOrId, string? key)
    {
        return await httpClient.GetStreamAsync($"icr/{key}/{collection}/{filenameOrId}");
    }

    public async Task<Result> UploadFileAsync(string collection, string filename, string contentType, Stream contentStream, string? key)
    {
        var content = new StreamContent(contentStream);
        content.Headers.Add("Content-Type", contentType);
        content.Headers.Add("X-FileName", filename);
        var response = await httpClient.PostAsync($"api/icr/{key}/{collection}/upload", content);
        if (response.IsSuccessStatusCode)
        {
            return Result.Ok();
        }
        else
        {
            return Result.Fail($"Unable to upload file. {response.StatusCode}.");
        }
    }

    public async Task<KeysAndPolicyModel> GetKeysAndPolicyAsync()
    {
        var responseMessage = await httpClient.GetAsync($"api/icr/keysandpolicy");
        if (responseMessage.IsSuccessStatusCode)
        {
            // Handle success
            var keysAndPolicy = await responseMessage.Content.ReadFromJsonAsync<KeysAndPolicyModel>();
            ArgumentNullException.ThrowIfNull(keysAndPolicy, nameof(keysAndPolicy));

            return keysAndPolicy;
        }

        throw new Exception($"Unable to query keys and policy limits.");
    }
}
