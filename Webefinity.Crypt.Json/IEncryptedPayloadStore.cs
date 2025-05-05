namespace Webefinity.Crypt.Json
{
    public interface IEncryptedPayloadStore
    {
        Task ClearValueAsync(string key);
        Task<bool> ContainsKeyAsync(string key);
        Task<EncryptedPayload?> GetEncryptedPayloadAsync(string key);
        Task SetEncryptedPayloadAsync(string key, EncryptedPayload payload);
        Task EvacuateCacheAsync(string? key = null);
        IAsyncEnumerable<string> EnumerateKeysAsync();
    }
}
