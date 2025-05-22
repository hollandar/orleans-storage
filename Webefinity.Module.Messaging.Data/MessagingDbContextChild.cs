using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Webefinity.Module.Messaging.Data;

public class MessagingDbContextChild<TDbContext> : IMessagingDbContext where TDbContext : DbContext
{
    private TDbContext context;

    public MessagingDbContextChild(TDbContext dbContext)
    {
        this.context = dbContext;
        Messages = dbContext.Set<Message>();
        Attachments = dbContext.Set<Attachment>();
        Addresses = dbContext.Set<Address>();
    }
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;

    public Task<int> SaveChangesAsync(CancellationToken ct = default!)
    {
        return this.context.SaveChangesAsync(ct);
    }

    public EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class
    {
        return this.context.Entry<TEntry>(entry);
    }
}
