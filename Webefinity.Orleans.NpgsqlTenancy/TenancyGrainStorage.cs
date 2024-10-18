using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.NpgsqlTenancy;
using Orleans.Runtime;
using Orleans.Storage;

namespace Webefinity.Orleans.NpgsqlTenancy;

public abstract class TenancyGrainStorage<TDbContext> : IGrainStorage where TDbContext : DbContext
{
    private string storageName;
    private IServiceProvider serviceProvider;
    private StorageDbFactory<TDbContext> storageDbFactory;

    protected TenancyGrainStorage(string storageName, IServiceProvider serviceProvider)
    {
        this.storageName = storageName;
        this.serviceProvider = serviceProvider;
        this.storageDbFactory = serviceProvider.GetRequiredService<StorageDbFactory<TDbContext>>();
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await this.storageDbFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        await ClearStateInternalAsync(stateName, context, grainId, grainState);
    }

    public abstract Task ClearStateInternalAsync<T>(string stateName, TDbContext context, GrainId grainId, IGrainState<T> grainState);

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await this.storageDbFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        await ReadStateInternalAsync(stateName, context, grainId, grainState);
    }

    public abstract Task ReadStateInternalAsync<T>(string stateName, TDbContext context, GrainId grainId, IGrainState<T> grainState);

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await this.storageDbFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        await WriteStateInternalAsync(stateName, context, grainId, grainState);
    }

    public abstract Task WriteStateInternalAsync<T>(string stateName, TDbContext context, GrainId grainId, IGrainState<T> grainState);
}
