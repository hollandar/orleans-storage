using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Webefinity.Crypt.Json;

public record EncryptedPayloadEntry(EncryptedPayload Payload, DateTimeOffset Expires);

public class EncryptedKeyValueService : IEncryptedKeyValueService
{
    private readonly byte[] encryptionKey = Array.Empty<byte>();
    private readonly IEncryptedPayloadStore encryptedPayloadStore;

    public EncryptedKeyValueService(byte[] encryptionKey, IEncryptedPayloadStore encryptedPayloadStore)
    {
        this.encryptionKey = encryptionKey;
        this.encryptedPayloadStore = encryptedPayloadStore;
    }

    public Task<bool> ContainsKeyAsync(string key) => encryptedPayloadStore.ContainsKeyAsync(key);
    
    public async Task<T?> GetValueAsync<T>(string key)
    {
        T? value;
        var encryptedPayload = await encryptedPayloadStore.GetEncryptedPayloadAsync(key);
        if (encryptedPayload == null)
        {
            return default;
        }

        using var aesCrypt = new AesCrypt(encryptionKey, encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        value = JsonSerializer.Deserialize<T>(decrypted, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        return value;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        string? value;

        var encryptedPayload = await encryptedPayloadStore.GetEncryptedPayloadAsync(key);
        if (encryptedPayload == null)
        {
            return null;
        }

        using var aesCrypt = new AesCrypt(encryptionKey, encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        value = Encoding.UTF8.GetString(decrypted);

        return value;
    }

    public Task ClearValueAsync(string key) => encryptedPayloadStore.ClearValueAsync(key);

    public async Task SetValueAsync<T>(string key, T value)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        using var aesCrypt = new AesCrypt(encryptionKey, iv);
        var encrypted = aesCrypt.Encrypt(Encoding.UTF8.GetBytes(json));
        var payload = new EncryptedPayload(iv, encrypted);
        await encryptedPayloadStore.SetEncryptedPayloadAsync(key, payload);
    }

    public async Task SetValueAsync(string key, string value)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        using var aesCrypt = new AesCrypt(encryptionKey, iv);
        var encrypted = aesCrypt.Encrypt(Encoding.UTF8.GetBytes(value));
        var payload = new EncryptedPayload(iv, encrypted);
        await encryptedPayloadStore.SetEncryptedPayloadAsync(key, payload);
    }

    public async Task EvacuateCacheAsync(string? key = null)
    {
        await encryptedPayloadStore.EvacuateCacheAsync(key);
    }

    public IAsyncEnumerable<string> EnumerateKeysAsync() => encryptedPayloadStore.EnumerateKeysAsync();
}
