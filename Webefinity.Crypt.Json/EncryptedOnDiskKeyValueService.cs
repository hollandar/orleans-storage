using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Webefinity.Crypt.Json.Options;

namespace Webefinity.Crypt.Json;

public record EncryptedPayloadEntry (EncryptedPayload Payload, DateTimeOffset Expires);

public class EncryptedOnDiskKeyValueService:IEncryptedKeyValueService
{
    private IDictionary<string, EncryptedPayloadEntry> encryptedPayloads;
    private readonly IOptions<EncryptedOnDiskOptions> options;
    private readonly ReaderWriterLockSlim semaphoreSlim = new ReaderWriterLockSlim();

    public EncryptedOnDiskKeyValueService(IOptions<EncryptedOnDiskOptions> options)
    {
        this.encryptedPayloads = new Dictionary<string, EncryptedPayloadEntry>();
        this.options = options;
        var path = options.Value.Path;
        if (!Directory.Exists(path))
        {
            throw new ArgumentException($"Path {path} is not valid.");
        }
    }

    private string GetFileName(string key) => $"{key}.epl";

    public bool ContainsKey(string key)
    {
        if (encryptedPayloads.ContainsKey(key) && encryptedPayloads[key].Expires > DateTimeOffset.UtcNow)
        {
            return true;
        }

        var payloadName = GetFileName(key);
        var payloadFile = Path.Combine(options.Value.Path, payloadName);
        return File.Exists(payloadFile);
    }

    public EncryptedPayload? GetEncryptedPayload(string key)
    {
        semaphoreSlim.EnterReadLock();
        try
        {
            if (encryptedPayloads.ContainsKey(key) && encryptedPayloads[key].Expires > DateTimeOffset.UtcNow)
            {
                return encryptedPayloads[key].Payload;
            }

            var payloadName = GetFileName(key);
            var payloadFile = Path.Combine(options.Value.Path, payloadName);
            if (!File.Exists(payloadFile))
            {
                return null;
            }

            var encryptedPayload = EncryptedPayloadWriter.ReadEncryptedPayload(payloadFile);
            this.encryptedPayloads[key] = new EncryptedPayloadEntry(encryptedPayload, DateTimeOffset.UtcNow.AddDays(1));

            return encryptedPayload;
        } finally
        {
            semaphoreSlim.ExitReadLock();
        }
    }

    public T? GetValue<T>(string key)
    {
        var encryptedPayload = GetEncryptedPayload(key);
        if (encryptedPayload == null)
        {
            return default;
        }   

        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        var jsonObject = JsonSerializer.Deserialize<T>(decrypted, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

        return jsonObject;
    }

    public string? GetValue(string key)
    {
        var encryptedPayload = GetEncryptedPayload(key);
        if (encryptedPayload == null)
        {
            return default;
        }   

        using var aesCrypt = new AesCrypt(Encoding.UTF8.GetBytes(options.Value.Key), encryptedPayload.Iv);
        var decrypted = aesCrypt.Decrypt(encryptedPayload.Bytes);
        var value = Encoding.UTF8.GetString(decrypted);

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
            this.encryptedPayloads[key] = new EncryptedPayloadEntry(payload, DateTimeOffset.UtcNow.AddDays(1));
            EvacuateCacheInternal(false);
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

    public void EvacuateCache(bool allValues = false)
    {
        semaphoreSlim.EnterWriteLock();
        try
        {
            EvacuateCacheInternal(allValues);
        }
        finally
        {
            semaphoreSlim.ExitWriteLock();
        }
    }

    private void EvacuateCacheInternal(bool allValues)
    {
        if (allValues)
        {
            encryptedPayloads.Clear();
        }
        else
        {
            var keysToRemove = encryptedPayloads.Where(kvp => kvp.Value.Expires < DateTimeOffset.UtcNow).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                encryptedPayloads.Remove(key);
            }
        }
    }
}
