using Microsoft.Extensions.Caching.Memory;

namespace Webefinity.Crypt.Json
{
    public class CacheWrapperEncryptedPayloadStore : IEncryptedPayloadStore
    {
        const int cacheNonVolatile = 30 * 60;
        private string GetPersistentCacheKey(string key) => "eps_persistent_" + key;
        private readonly IEncryptedPayloadStore innerPayloadStore;
        private readonly IMemoryCache? memoryCache;

        public CacheWrapperEncryptedPayloadStore(IEncryptedPayloadStore innerPayloadStore, IMemoryCache? memoryCache)
        {
            this.innerPayloadStore = innerPayloadStore;
            this.memoryCache = memoryCache;
        }

        public async Task SetEncryptedPayloadAsync(string key, EncryptedPayload payload)
        {
            var cacheKey = GetPersistentCacheKey(key);

            await this.innerPayloadStore.SetEncryptedPayloadAsync(key, payload);
            this.memoryCache?.Set(cacheKey, payload, DateTimeOffset.UtcNow.AddSeconds(cacheNonVolatile));
        }

        public Task<EncryptedPayload?> GetEncryptedPayloadAsync(string key)
        {
            if (memoryCache?.TryGetValue(GetPersistentCacheKey(key), out EncryptedPayload? payload) ?? false)
            {
                return Task.FromResult(payload);
            }

            return this.innerPayloadStore.GetEncryptedPayloadAsync(key);
        }

        public Task ClearValueAsync(string key)
        {
            memoryCache?.Remove(GetPersistentCacheKey(key));
            return innerPayloadStore.ClearValueAsync(key);
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            if (memoryCache?.TryGetValue(GetPersistentCacheKey(key), out _) ?? false)
            {
                return true;
            }

            return await innerPayloadStore.ContainsKeyAsync(key);
        }

        public async Task EvacuateCacheAsync(string? key = null)
        {
            if (key is not null)
            {
                memoryCache?.Remove(GetPersistentCacheKey(key));
            }
            else
            {
                await foreach (var cacheKey in EnumerateKeysAsync())
                {
                    if (cacheKey.StartsWith("eps_persistent_"))
                    {
                        memoryCache?.Remove(cacheKey);
                    }
                }
            }

            await this.innerPayloadStore.EvacuateCacheAsync(key);
        }

        public IAsyncEnumerable<string> EnumerateKeysAsync()
        {
            return innerPayloadStore.EnumerateKeysAsync();
        }
    }
}
