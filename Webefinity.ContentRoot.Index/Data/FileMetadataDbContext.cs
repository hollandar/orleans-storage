using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.ContentRoot.Index.Data
{
    public class FileMetadataDbContext: DbContext, IFileMetadataDbContext
    {
        public DbSet<FileMetadata> Files { get; set; }
        public DbSet<Metadata> Metadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddFileIndexModel();
            base.OnModelCreating(modelBuilder);
        }
    }

    public class FileMetadataDbContextChild<TDbContext> : IFileMetadataDbContext where TDbContext : DbContext
    {
        private readonly TDbContext dbContext;

        public FileMetadataDbContextChild(TDbContext context)
        {
            dbContext = context;
            Files = context.Set<FileMetadata>();
            Metadata = context.Set<Metadata>();

        }

        public DbSet<FileMetadata> Files { get; }
        public DbSet<Metadata> Metadata { get; }

        public Task<int> SaveChangesAsync(CancellationToken ct = default!)
        {
            return dbContext.SaveChangesAsync(ct);
        }

        public EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class
        {
            return dbContext.Entry(entry);
        }
    }
}
