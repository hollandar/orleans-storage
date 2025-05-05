using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.Crypt.Json
{
    public class VolatileCacheEncryptedKeyValueService : IEncryptedKeyValueService
    {
        const int cacheVolatile = 5;
        private readonly IEncryptedKeyValueService innerKeyValueService;
        private readonly IMemoryCache? memoryCache;

        public VolatileCacheEncryptedKeyValueService(IEncryptedKeyValueService innerKeyValueService, IMemoryCache? memoryCache)
        {
            this.innerKeyValueService = innerKeyValueService;
            this.memoryCache = memoryCache;
        }

        private string GetVolatileCacheKey(string key) => "eps_volatile_" + key;

        public Task ClearValueAsync(string key)
        {
            memoryCache?.Remove(GetVolatileCacheKey(key));
            return innerKeyValueService.ClearValueAsync(key);
        }

        public async Task<bool> ContainsKeyAsync(string key)
        {
            if (memoryCache?.TryGetValue(GetVolatileCacheKey(key), out _) ?? false)
            {
                return true;
            }

            return await innerKeyValueService.ContainsKeyAsync(key);
        }

        public IAsyncEnumerable<string> EnumerateKeysAsync()
        {
            return innerKeyValueService.EnumerateKeysAsync();
        }

        public async Task EvacuateCacheAsync(string? key = null)
        {
            if (key is null)
            {
                await foreach (var innerKey in EnumerateKeysAsync())
                {
                    memoryCache?.Remove(GetVolatileCacheKey(innerKey));
                }
            }
            else
            {
                memoryCache?.Remove(GetVolatileCacheKey(key));
            }
        }

        public Task<T?> GetValueAsync<T>(string key)
        {
            if (memoryCache?.TryGetValue(GetVolatileCacheKey(key), out T? value) ?? false)
            {
                return Task.FromResult(value);
            }

            return innerKeyValueService.GetValueAsync<T>(key);
        }

        public Task<string?> GetValueAsync(string key)
        {
            if (memoryCache?.TryGetValue(GetVolatileCacheKey(key), out string? value) ?? false)
            {
                return Task.FromResult(value);
            }

            return innerKeyValueService.GetValueAsync(key);
        }

        public async Task SetValueAsync<T>(string key, T value)
        {
            await innerKeyValueService.SetValueAsync(key, value);
            memoryCache?.Set(GetVolatileCacheKey(key), value, DateTimeOffset.UtcNow.AddSeconds(cacheVolatile));
        }

        public async Task SetValueAsync(string key, string value)
        {
            await innerKeyValueService.SetValueAsync(key, value);
            memoryCache?.Set(GetVolatileCacheKey(key), value, DateTimeOffset.UtcNow.AddSeconds(cacheVolatile));
        }
    }
}
