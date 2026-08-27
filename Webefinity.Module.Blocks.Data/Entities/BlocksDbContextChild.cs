using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Blocks.Data.Entities;

public interface IBlocksDbContext
{
    DbSet<Block> Blocks { get; set; }
    DbSet<Page> Pages { get; set; }

    Task<int> SaveChangesAsync(CancellationToken ct = default!);
}

public class BlocksDbContext<TDbContext> : IBlocksDbContext where TDbContext : DbContext
{
    private readonly TDbContext dbContext;

    public DbSet<Block> Blocks { get; set; }
    public DbSet<Page> Pages { get; set; }

    public BlocksDbContext(TDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.Blocks = this.dbContext.Set<Block>();
        this.Pages = this.dbContext.Set<Page>();
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default!) { return await dbContext.SaveChangesAsync(ct); }

}

public static class BlocksDbContextExtensions
{
    public static void AddBlocksEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>(builder =>
        {
            builder.HasKey(r => r.Id);
        });

        modelBuilder.Entity<Block>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.HasIndex(r => new { r.PageId, r.Sequence }).IsUnique();
            builder.HasOne(r => r.Page).WithMany(r => r.Blocks).HasForeignKey(r => r.PageId);//.OnDelete(DeleteBehavior.Cascade);
        });
    }
}
