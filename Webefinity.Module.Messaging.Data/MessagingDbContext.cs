using Microsoft.EntityFrameworkCore;
using System.Formats.Tar;

namespace Webefinity.Module.Messaging.Data;

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
