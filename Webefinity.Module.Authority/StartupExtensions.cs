using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Authority.Entities;
using Webefinity.Module.Authority.Services;

namespace Webefinity.Module.Authority;

public static class StartupExtensions
{
    public static void AddAuthorityDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddScoped<IAuthorityDbContext, AuthorityDbContextChild<TDbContext>>(sp =>
        {
            var dbContext = sp.GetRequiredService<TDbContext>();
            return new AuthorityDbContextChild<TDbContext>(dbContext);
        });
        services.AddScoped<AuthorityService>();
    }
    
    public static void AddAuthorityToModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAuthority>(builder =>
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.UserId).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Name).HasMaxLength(100);
            builder.Property(r => r.Value).IsRequired().HasMaxLength(200);
            builder.Property(r => r.Type).IsRequired();
        });
    }
}
