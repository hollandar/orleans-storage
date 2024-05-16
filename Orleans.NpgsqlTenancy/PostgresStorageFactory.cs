﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;
using Orleans.Storage;

namespace Orleans.NpgsqlTenancy;

public static class PostgresStorageFactory
{
    public static IGrainStorage Create(
        IServiceProvider services, string name)
    {
        return ActivatorUtilities.CreateInstance<PostgresStorageProvider>(
            services,
            name,
            services.GetProviderClusterOptions(name));
    }
}
