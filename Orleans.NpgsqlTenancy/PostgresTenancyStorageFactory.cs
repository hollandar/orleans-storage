using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace Orleans.NpgsqlTenancy;

public static class PostgresTenancyStorageFactory<TDbContext> where TDbContext: GrainStoreDbContext
{
    public static IGrainStorage Create(
        IServiceProvider services, string name)
    {
        return ActivatorUtilities.CreateInstance<PostgresTenancyStorageProvider<TDbContext>>(
            services,
            name,
            services.GetProviderClusterOptions(name));
    }
}
