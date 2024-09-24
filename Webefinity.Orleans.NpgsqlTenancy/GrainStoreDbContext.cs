using Microsoft.EntityFrameworkCore;

namespace Orleans.NpgsqlTenancy;

public abstract class GrainStoreDbContext: DbContext
{
    public GrainStoreDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<GrainStateType> States { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GrainStateType>().HasKey(r => new { r.Id, r.StateName });
        modelBuilder.Entity<GrainStateType>().Property(r => r.ETag).HasMaxLength(64).IsConcurrencyToken();
        base.OnModelCreating(modelBuilder);
    }
}
