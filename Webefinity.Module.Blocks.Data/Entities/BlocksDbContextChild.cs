using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Blocks.Data.Entities;

public interface IBlocksDbContextChild
{
    DbSet<Block> Blocks { get; }
    DbSet<Page> Pages { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default!);
}

public class BlocksDbContextChild<TDbContext> : IBlocksDbContextChild where TDbContext : DbContext
{
    private readonly TDbContext dbContext;

    public BlocksDbContextChild(TDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public DbSet<Block> Blocks => dbContext.Set<Block>();
    public DbSet<Page> Pages => dbContext.Set<Page>();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default!) { return await dbContext.SaveChangesAsync(ct); }
}
