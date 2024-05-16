using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using System.Text.Json;

namespace Orleans.NpgsqlTenancy;

public class PostgresStorageProvider : IGrainStorage
{
    private readonly IServiceProvider services;
    private readonly string storageName;
    private readonly IOptions<ClusterOptions> clusterOptions;

    public PostgresStorageProvider(
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
        var storageFactory = services.GetRequiredService<StorageDbFactory<StateDbContext>>();
        using var context = await storageFactory.GetDbContext(storageName);

        await context.States.Where(r => r.StateName == stateName && r.Id == grainId.ToString()).ExecuteDeleteAsync();
    }

    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var storageFactory = services.GetRequiredService<StorageDbFactory<StateDbContext>>();
        using var context = await storageFactory.GetDbContext(storageName);

        var state = context.States.FirstOrDefault(r => r.StateName == stateName && r.Id == grainId.ToString());
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
        var storageFactory = services.GetRequiredService<StorageDbFactory<StateDbContext>>();
        using var context = await storageFactory.GetDbContext(storageName);

        var state = context.States.FirstOrDefault(r => r.StateName == stateName && r.Id == grainId.ToString());
        if (state is null)
        {
            state = new GrainStateType();
            state.StateName = stateName;
            state.Id = grainId.ToString();
            context.States.Add(state);
        }

        state.ETag = Guid.NewGuid().ToString();
        state.State = JsonSerializer.SerializeToDocument(grainState.State);

        await context.SaveChangesAsync();
    }
}
