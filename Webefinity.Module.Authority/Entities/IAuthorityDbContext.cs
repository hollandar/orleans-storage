using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Authority.Entities;

public interface IAuthorityDbContext
{
    DbSet<UserAuthority> Authorities { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
