using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Webefinity.Crypt.Json.Options;

namespace Webefinity.Crypt.Json;

public record EncryptedPayloadEntry(EncryptedPayload Payload, DateTimeOffset Expires);

public class EncryptedOnDiskKeyValueService : IEncryptedKeyValueService
{
    const int cacheNonVolatile = 30 * 60;
    const int cacheVolatile = 5;
    private HashSet<string> knownKeys = new();
    private readonly IOptions<EncryptedOnDiskOptions> options;
    private readonly ReaderWriterLockSlim semaphoreSlim = new ReaderWriterLockSlim();
    private readonly IMemoryCache? memoryCache;

    public EncryptedOnDiskKeyValueService(IOptions<EncryptedOnDiskOptions> options, IMemoryCache memoryCache)
    {
        this.knownKeys = new HashSet<string>();
        this.options = options;
        var path = options.Value.Path;
        if (!Directory.Exists(path))
        {
            throw new ArgumentException($"Path {path} is not valid.");
        }
        this.memoryCache = memoryCache;
    }

    private string GetFileName(string key) => $"{key}.epl";

    public bool ContainsKey(string key)
    {
        if (this.knownKeys.Contains(key))
        {
            return true;
        }

        var payloadName = GetFileName(key);
        var payloadFile = Path.Combine(options.Value.Path, payloadName);
        var keyExists = File.Exists(payloadFile);
        if (keyExists)
        {
            this.knownKeys.Add(key);
        }

        return keyExists;
    }

    public EncryptedPayload? GetEncryptedPayload(string key)
    {
        semaphoreSlim.EnterReadLock();
        try
        {
            EncryptedPayload? encryptedPayload;
            string cacheKey = $"encryptedPayload_{key}";
            if (this.memoryCache?.TryGetValue(key, out encryptedPayload) ?? false)
            {
                return encryptedPayload;
            }

            var payloadName = GetFileName(key);
            var payloadFile = Path.Combine(options.Value.Path, payloadName);
            if (!File.Exists(payloadFile))
            {
                return null;
            }

            encryptedPayload = EncryptedPayloadWriter.ReadEncryptedPayload(payloadFile);
            this.memoryCache?.Set(cacheKey, encryptedPayload, DateTimeOffset.UtcNow.AddSeconds(cacheNonVolatile));

            return encryptedPayload;
        }
        finally
        {
            semaphoreSlim.ExitReadLock();
        }
    }

    public T? GetValue<T>(string key)
    {
        T? value;
        string cacheKey = $"cryptjson_unencryptedValue_T_{key}";
        if (this.memoryCache?.TryGetValue(cacheKey, out value) ?? false)
        {
            return value;
        }

        var encryptedPayload = GetEncryptedPayload(key);
        if (encryptedPayload == null)
        {
            return default;
        }

        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        value = JsonSerializer.Deserialize<T>(decrypted, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        this.memoryCache?.Set(cacheKey, value, DateTimeOffset.UtcNow.AddSeconds(cacheVolatile));
        return value;
    }

    public string? GetValue(string key)
    {
        string cacheKey = $"cryptjson_unencryptedValue_str_{key}";
        string? value;
        if (this.memoryCache?.TryGetValue(cacheKey, out value) ?? false)
        {
            return value;
        }

        var encryptedPayload = GetEncryptedPayload(key);
        if (encryptedPayload == null)
        {
            return default;
        }

        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        value = Encoding.UTF8.GetString(decrypted);
        this.memoryCache?.Set(cacheKey, value, DateTimeOffset.UtcNow.AddSeconds(cacheVolatile));

        return value;
    }

    public void SetEncryptedPayload(string key, EncryptedPayload payload)
    {
        semaphoreSlim.EnterWriteLock();
        try
        {
            var payloadName = GetFileName(key);
            var payloadFile = Path.Combine(options.Value.Path, payloadName);
            EncryptedPayloadWriter.WriteEncryptedPayload(payloadFile, payload);
            EvacuateCache(key);
        }
        finally
        {
            semaphoreSlim.ExitWriteLock();
        }
    }

    public  Task ClearValueAsync(string key)
    {
        try
        {
            semaphoreSlim.EnterWriteLock();
            var payloadName = GetFileName(key);
            var payloadFile = Path.Combine(options.Value.Path, payloadName);
            if (File.Exists(payloadFile))
            {
                File.Delete(payloadFile);
            }
            EvacuateCache(key);

            return Task.CompletedTask;
        }
        finally
        {
            semaphoreSlim.ExitWriteLock();
        }
    }

    public void SetValue<T>(string key, T value)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), iv);
        var encrypted = aesCrypt.Encrypt(Encoding.UTF8.GetBytes(json));
        var payload = new EncryptedPayload(iv, encrypted);
        SetEncryptedPayload(key, payload);
    }

    public void SetValue(string key, string value)
    {
        var iv = RandomNumberGenerator.GetBytes(16);
        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), iv);
        var encrypted = aesCrypt.Encrypt(Encoding.UTF8.GetBytes(value));
        var payload = new EncryptedPayload(iv, encrypted);
        SetEncryptedPayload(key, payload);
    }

    public void EvacuateCache(string? key = null)
    {
        if (key is null)
        {
            foreach (string innerKey in this.knownKeys) EvacuateCache(key);
            this.knownKeys.Clear();
        }
        else
        {
            memoryCache?.Remove($"cryptjson_unencryptedValue_str_{key}");
            memoryCache?.Remove($"cryptjson_unencryptedValue_T_{key}");
            memoryCache?.Remove($"encryptedPayload_{key}");
        }
    }
}
