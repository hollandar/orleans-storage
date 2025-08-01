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
        this.Blocks = this.dbContext.Set<Block>();
        this.Pages = this.dbContext.Set<Page>();
    }

    public DbSet<Block> Blocks { get; private set; }
    public DbSet<Page> Pages { get; private set; }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default!) { return await dbContext.SaveChangesAsync(ct); }
}
