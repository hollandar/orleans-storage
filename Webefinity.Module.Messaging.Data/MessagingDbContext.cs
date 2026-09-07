using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Formats.Tar;

namespace Webefinity.Module.Messaging.Data;

public class MessagingDbContext<TDbContext> : IMessagingDbContext where TDbContext : DbContext
{
    private TDbContext baseDbContext;

    public MessagingDbContext(TDbContext dbContext)
    {
        this.baseDbContext = dbContext;
        this.Messages = this.baseDbContext.Set<Message>();
        this.Attachments = this.baseDbContext.Set<Attachment>();
        this.Addresses = this.baseDbContext.Set<Address>();
    }
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Attachment> Attachments { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;

    public EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class
    {
        return this.baseDbContext.Entry(entry);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return this.baseDbContext.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        this.baseDbContext.Dispose();
    }
}

public static class MessagingDbContextExtensions
{
    public static void AddMessagingEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(builder =>
        {
            builder.ToTable("Messages");
            builder.HasKey(m => m.Id);
            builder.HasIndex(m => new { m.SenderId });
            builder.HasIndex(m => new { m.Created });
            builder.HasIndex(m => new { m.Status, m.RetryAfter, m.RetryCount });
            builder.HasIndex(m => new { m.PurgeAfter });
            builder.HasMany(m => m.Attachments).WithOne(a => a.Message).HasForeignKey(a => a.AttachmentId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(m => m.Addresses).WithOne(a => a.Message).HasForeignKey(a => a.MessageId).OnDelete(DeleteBehavior.Cascade);
            builder.Property(r => r.RetryAfter).HasDefaultValue(null);
            builder.Property(r => r.RetryCount).HasDefaultValue(10);
        });

        modelBuilder.Entity<Attachment>(builder =>
        {
            builder.ToTable("Attachments");
            builder.HasKey(a => a.Id);
        });

        modelBuilder.Entity<Address>(builder =>
        {
            builder.ToTable("Addresses");
            builder.HasKey(a => a.Id);
        });
    }
}


