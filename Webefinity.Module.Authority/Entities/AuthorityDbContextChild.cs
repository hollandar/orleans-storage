using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Authority.Entities;

public class AuthorityDbContextChild<TDbContext> : IAuthorityDbContext where TDbContext : DbContext
{

    private readonly TDbContext dbContext;

    public DbSet<UserAuthority> Authorities { get; init; }

    public AuthorityDbContextChild(TDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.Authorities = dbContext.Set<Entities.UserAuthority>();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
