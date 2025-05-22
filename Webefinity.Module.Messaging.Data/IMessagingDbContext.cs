using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Webefinity.Module.Messaging.Data;

public interface IMessagingDbContext
{
    DbSet<Message> Messages { get; set; }
    DbSet<Attachment> Attachments { get; set; }
    DbSet<Address> Addresses { get; set; }

    EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
