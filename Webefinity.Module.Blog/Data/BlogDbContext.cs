using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Webefinity.Module.Blog.Data;

public interface IBlogDbContext
{
    DbSet<BlogArticle> Articles { get; set; }
    DbSet<BlogTag> Tags { get; set; }
    DbSet<BlogWord> Words { get; set; }
    EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

internal class BlogDbContext<TDbContext> : IBlogDbContext where TDbContext : DbContext
{
    private readonly TDbContext baseDbContext;

    public BlogDbContext(TDbContext baseDbContext)
    {
        this.baseDbContext = baseDbContext;
        this.Articles = baseDbContext.Set<BlogArticle>();
        this.Tags = baseDbContext.Set<BlogTag>();
        this.Words = baseDbContext.Set<BlogWord>();
    }

    public DbSet<BlogArticle> Articles { get; set; } = null!;
    public DbSet<BlogTag> Tags { get; set; } = null!;
    public DbSet<BlogWord> Words { get; set; } = null!;

    public EntityEntry<TEntry> Entry<TEntry>(TEntry entry) where TEntry : class
    {
        return this.baseDbContext.Entry(entry);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return this.baseDbContext.SaveChangesAsync(ct);
    }
}

public static class BlogDbContextExtensions
{
    extension(ModelBuilder modelBuilder)
    {
        public void AddBlogEntities()
        {
            modelBuilder.Entity<BlogArticle>(entity =>
            {
                entity.ToTable("BlogArticles");
                entity.HasKey(e => e.Id);
                entity.HasMany(e => e.Tags).WithOne(e => e.Article).HasForeignKey(e => e.ArticleId);
                entity.HasMany(e => e.Words).WithOne(e => e.Article).HasForeignKey(e => e.ArticleId);
            });

            modelBuilder.Entity<BlogTag>(entity =>
            {
                entity.ToTable("BlogTags");
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Tag);
            });

            modelBuilder.Entity<BlogWord>(entity =>
            {
                entity.ToTable("BlogWords");
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Word);
            });

        }
    }
}
