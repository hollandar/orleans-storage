using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Authority.Entities;

public interface IAuthorityDbContext
{
    DbSet<UserAuthority> Authorities { get; init; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
