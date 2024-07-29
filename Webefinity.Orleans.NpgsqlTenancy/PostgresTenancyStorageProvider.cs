using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Orleans.NpgsqlTenancy;

public class PostgresTenancyStorageProvider<TDbContext> : IGrainStorage where TDbContext: GrainStoreDbContext
{
    private readonly IServiceProvider services;
    private readonly string storageName;
    private readonly IOptions<ClusterOptions> clusterOptions;

    public PostgresTenancyStorageProvider(
        string storageName,
        IServiceProvider services,
        IOptions<ClusterOptions> clusterOptions)
    {
        this.storageName = storageName;
        this.services = services;
        this.clusterOptions = clusterOptions;
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storageFactory = services.GetRequiredService<StorageDbFactory<TDbContext>>();
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await storageFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        await context.States.Where(r => r.StateName == stateName && r.Id == tenancy.Id).ExecuteDeleteAsync();
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storageFactory = services.GetRequiredService<StorageDbFactory<TDbContext>>();
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await storageFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        var state = context.States.FirstOrDefault(r => r.StateName == stateName && r.Id == tenancy.Id);
        if (state is not null)
        {
            var deserializedState = state.State.Deserialize<T>();
            if (deserializedState is not null)
            {
                grainState.ETag = state.ETag;
                grainState.State = deserializedState;
                grainState.RecordExists = true;

                return;
            }
        }

        grainState.RecordExists = false;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storageFactory = services.GetRequiredService<StorageDbFactory<TDbContext>>();
        var tenancy = TenancyKey.Parse(grainId);
        using var context = await storageFactory.GetTenantDbContext(storageName, tenancy.TenantId);

        var state = context.States.FirstOrDefault(r => r.StateName == stateName && r.Id == tenancy.Id);
        if (state is null)
        {
            state = new GrainStateType();
            state.StateName = stateName;
            state.Id = tenancy.Id;
            context.States.Add(state);
        }

        state.ETag = Guid.NewGuid().ToString();
        state.State = JsonSerializer.SerializeToDocument(grainState.State);

        await context.SaveChangesAsync();
    }
}
