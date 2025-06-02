
namespace Webefinity.Crypt.Json;

public interface IEncryptedKeyValueService
{
    Task ClearValueAsync(string key);
    Task<bool> ContainsKeyAsync(string key);
    Task EvacuateCacheAsync(string? key = null);
    Task<T?> GetValueAsync<T>(string key, bool mustExist = true);
    Task<string?> GetValueAsync(string key, bool mustExist = false);
    Task SetValueAsync<T>(string key, T value);
    Task SetValueAsync(string key, string value);
    IAsyncEnumerable<string> EnumerateKeysAsync();
}
