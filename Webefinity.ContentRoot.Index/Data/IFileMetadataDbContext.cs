using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.ContentRoot.Index.Data;

public interface IFileMetadataDbContext
{
    DbSet<FileMetadata> Files { get; }
    DbSet<Metadata> Metadata { get; }

    EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
