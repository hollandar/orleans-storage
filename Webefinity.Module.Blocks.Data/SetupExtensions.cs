using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Blocks.Data.Entities;
using Webefinity.Module.Blocks.Data.Services;
using Webefinity.Modules.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Data;

public static class SetupExtensions
{
    public static void AddBlocksDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IBlocksDbContextChild, BlocksDbContextChild<TDbContext>>(sp => { 
            var dbContext = sp.GetRequiredService<TDbContext>();
            return new BlocksDbContextChild<TDbContext>(dbContext);
        });
        services.AddScoped<IBlocksDataProvider, BlocksDataService>();
    }

    public static void AddBlocksToModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>(builder => {
            builder.HasKey(r => r.Id);
        });

        modelBuilder.Entity<Block>(builder => {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => new { r.PageId, r.Sequence }).IsUnique();
            builder.HasOne(r => r.Page).WithMany(r => r.Blocks).HasForeignKey(r => r.PageId);
        });
    }
}
