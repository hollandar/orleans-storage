using Microsoft.EntityFrameworkCore;

namespace Webefinity.Module.Messaging.Data;

public interface IMessagingDbContext
{
    DbSet<Message> Messages { get; set; }
    DbSet<Attachment> Attachments { get; set; }
    DbSet<Address> Addresses { get; set; }
}

public class MessagingDbContext : DbContext, IMessagingDbContext
{
    public MessagingDbContext(DbContextOptions<MessagingDbContext> options) : base(options) { }
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddMessagingModel();
    }
}
public class MessagingDbContextChild<TDbContext> : IMessagingDbContext where TDbContext: DbContext
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

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return this.context.SaveChangesAsync(ct);
    }
}
