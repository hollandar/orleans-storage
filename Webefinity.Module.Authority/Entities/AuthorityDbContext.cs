using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Authority.Entities;

public class AuthorityDbContext<TDbContext> : IAuthorityDbContext where TDbContext : DbContext
{

    private readonly TDbContext dbContext;
    public DbSet<UserAuthority> Authorities { get; set; }

    public AuthorityDbContext(TDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.Authorities = dbContext.Set<UserAuthority>();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

public static class AuthorityDbContextExtensions
{
    public static void AddAuthorityEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuthority>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.UserId).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Name).HasMaxLength(100);
            builder.Property(r => r.Value).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Type).IsRequired();
        });
    }
}
