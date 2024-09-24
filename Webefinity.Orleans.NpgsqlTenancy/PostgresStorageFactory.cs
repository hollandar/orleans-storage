using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace Orleans.NpgsqlTenancy;

public static class PostgresStorageConstants
{
    public const string StorageName = "StateStorage";
}

public static class PostgresStorageFactory<TDbContext> where TDbContext: StateDbContext
{
    public static IGrainStorage Create(
        IServiceProvider services, string name)
    {
        return ActivatorUtilities.CreateInstance<PostgresStorageProvider<TDbContext>>(
            services,
            name,
            services.GetProviderClusterOptions(name));
    }
}
