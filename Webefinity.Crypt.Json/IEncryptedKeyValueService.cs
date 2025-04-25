namespace Webefinity.Crypt.Json;

public interface IEncryptedKeyValueService
{
    bool ContainsKey(string key);
    void EvacuateCache(bool allValues = false);
    EncryptedPayload? GetEncryptedPayload(string key);
    T? GetValue<T>(string key);
    void SetEncryptedPayload(string key, EncryptedPayload payload);
    void SetValue<T>(string key, T value);
}
