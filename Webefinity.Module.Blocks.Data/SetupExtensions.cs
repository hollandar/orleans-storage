using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;
using Webefinity.Module.Blocks.Data.Services;

namespace Webefinity.Module.Blocks.Data;

public static class SetupExtensions
{
    public static void AddBlocksDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IBlocksDbContext, BlocksDbContext<TDbContext>>(sp => { 
            var dbContext = sp.GetRequiredService<TDbContext>();
            return new BlocksDbContext<TDbContext>(dbContext);
        });
        services.AddScoped<IBlocksDataProvider, BlocksDataService>();
    }
}
