
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.NpgsqlTenancy.Options;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace Orleans.NpgsqlTenancy;

public class StorageDbFactory<TDbContext> where TDbContext : DbContext
{
    static ConcurrentDictionary<string, string> exists = new();
    static SemaphoreSlim creationSemaphore = new SemaphoreSlim(1);
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly IOptions<TenancyStorageOptions> tenancyDbOptions;

    public StorageDbFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
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
            var connectionStringSetting = configuration.GetConnectionString(key);
            if (connectionStringSetting is not null)
            {
                connectionString = connectionStringSetting;
                exists[key] = connectionString;
                migrate = true;
            }
            else
            {
                var templateConnectionString = configuration.GetConnectionString("Tenant");
                if (templateConnectionString is null || !templateConnectionString.Contains("{database}"))
                {
                    throw new ArgumentException("The Tenant connection string does not exist, or does not contain the {database} replacement flag.");
                }
                var database = tenancyId switch
                {
                    null => storageName,
                    _ => $"{storageName}_{tenancyId}".Replace("-", ""),
                };

                connectionString = templateConnectionString.Replace("{database}", database);
                exists[key] = connectionString;
                migrate = true;
            }
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
