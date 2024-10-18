
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.NpgsqlTenancy.Options;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace Orleans.NpgsqlTenancy;

public class StorageDbFactory<TDbContext> where TDbContext: DbContext
{
    static ConcurrentDictionary<string, string> exists = new();
    static SemaphoreSlim creationSemaphore = new SemaphoreSlim(1);
    private readonly IServiceProvider serviceProvider;
    private readonly IOptions<TenancyStorageOptions> tenancyDbOptions;

    public StorageDbFactory(IServiceProvider serviceProvider, IOptions<TenancyStorageOptions> tenancyDbOptions)
    {
        this.serviceProvider = serviceProvider;
        this.tenancyDbOptions = tenancyDbOptions;
    }

    public Task<TDbContext> GetDbContext(string storageName) => GetDbContextInternal(storageName);
    
    public Task<TDbContext> GetTenantDbContext(string storageName, string tenancyId) => GetDbContextInternal(storageName, tenancyId);
    
    protected async Task<TDbContext> GetDbContextInternal(string storageName, string? tenancyId = null)
    {
        var key = storageName + tenancyId ?? string.Empty;
        string? connectionString;
        bool migrate = false;
        if (!exists.TryGetValue(key, out connectionString))
        {
            Contract.Assert(this.tenancyDbOptions.Value.Storages.ContainsKey(storageName), $"Storage {storageName} not found.");
            var storage = this.tenancyDbOptions.Value.Storages[storageName];
            var database = tenancyId switch
            {
                null => storage.Database,
                _ => $"{storageName}_{tenancyId}".Replace("-", ""),
            };

            connectionString = $"Host={storage.Host};Database={database};User Id={storage.UserId};Password={storage.Password};Port={storage.Port}";
            exists[key] = connectionString;
            migrate = true;
        }

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseApplicationServiceProvider(serviceProvider);
        var newContext = ActivatorUtilities.CreateInstance<TDbContext>(serviceProvider, optionsBuilder.Options);

        if (migrate)
        {
            await creationSemaphore.WaitAsync();
            try
            {
                await newContext.Database.MigrateAsync();
            }
            finally
            {
                creationSemaphore.Release();
            }
        }

        return newContext;
    }
}
