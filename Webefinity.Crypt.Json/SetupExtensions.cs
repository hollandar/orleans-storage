using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using Webefinity.Crypt.Json.Options;

namespace Webefinity.Crypt.Json;

public static class SetupExtensions
{
    public static IServiceCollection AddOnDiskEncryptedKeyValueService(this IServiceCollection services, string? key = null, string? configurationSection = null)
    {
        
        if (key is null)
        {
            services.AddSingleton<IEncryptedKeyValueService>((sp) => {
                var options = Microsoft.Extensions.Options.Options.Create(new EncryptedOnDiskOptions());
                var configuration = sp.GetService<IConfiguration>();
                if (configurationSection is null)
                {
                    options = sp.GetRequiredService<IOptions<EncryptedOnDiskOptions>>();
                    var onDisk = new OnDiskEncryptedPayloadStore(options.Value.Path);
                    var cache = new CacheWrapperEncryptedPayloadStore(onDisk, sp.GetService<IMemoryCache>());
                    var service = new EncryptedKeyValueService(Encoding.UTF8.GetBytes(options.Value.Key), cache);
                    var cacheService = new VolatileCacheEncryptedKeyValueService(service, sp.GetService<IMemoryCache>());
                    return cacheService;
                }
                else
                {
                    var section = configuration.GetRequiredSection(configurationSection);
                    section.Bind(options.Value);
                    var onDisk = new OnDiskEncryptedPayloadStore(options.Value.Path);
                    var cache = new CacheWrapperEncryptedPayloadStore(onDisk, sp.GetService<IMemoryCache>());
                    var service = new EncryptedKeyValueService(Encoding.UTF8.GetBytes(options.Value.Key), cache);
                    var cacheService = new VolatileCacheEncryptedKeyValueService(service, sp.GetService<IMemoryCache>());
                    return cacheService;
                }
            });
        }
        else
        {
            services.AddKeyedSingleton<IEncryptedKeyValueService>(key, (sp, _) => {
                var options = Microsoft.Extensions.Options.Options.Create(new EncryptedOnDiskOptions());
                var configuration = sp.GetService<IConfiguration>();
                if (configurationSection is null)
                {
                    options = sp.GetRequiredService<IOptions<EncryptedOnDiskOptions>>();
                    var onDisk = new OnDiskEncryptedPayloadStore(options.Value.Path);
                    var cache = new CacheWrapperEncryptedPayloadStore(onDisk, sp.GetService<IMemoryCache>());
                    var service = new EncryptedKeyValueService(Encoding.UTF8.GetBytes(options.Value.Key), cache);
                    var cacheService = new VolatileCacheEncryptedKeyValueService(service, sp.GetService<IMemoryCache>());
                    return cacheService;
                }
                else
                {
                    var section = configuration.GetRequiredSection(configurationSection);
                    section.Bind(options.Value);

                    var onDisk = new OnDiskEncryptedPayloadStore(options.Value.Path);
                    var cache = new CacheWrapperEncryptedPayloadStore(onDisk, sp.GetService<IMemoryCache>());
                    var service = new EncryptedKeyValueService(Encoding.UTF8.GetBytes(options.Value.Key), cache);
                    var cacheService = new VolatileCacheEncryptedKeyValueService(service, sp.GetService<IMemoryCache>());
                    return cacheService;
                }
            });

        }

        services.AddMemoryCache();
        return services;
    }
}
