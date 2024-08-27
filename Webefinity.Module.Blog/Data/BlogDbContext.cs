using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Webefinity.Module.Blog.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options)
    {

    }

    public DbSet<BlogArticle> Articles { get; set; } = null!;
    public DbSet<BlogTag> Tags { get; set; } = null!;
    public DbSet<BlogWord> Words { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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


        base.OnModelCreating(modelBuilder);
    }
}
