
namespace Webefinity.Crypt.Json;

public interface IEncryptedKeyValueService
{
    Task ClearValueAsync(string key);
    bool ContainsKey(string key);
    void EvacuateCache(string? key = null);
    EncryptedPayload? GetEncryptedPayload(string key);
    T? GetValue<T>(string key);
    string? GetValue(string key);
    void SetEncryptedPayload(string key, EncryptedPayload payload);
    void SetValue<T>(string key, T value);
    void SetValue(string key, string value);
}
