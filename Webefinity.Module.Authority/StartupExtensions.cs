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
        services.AddScoped<IAuthorityDbContext, AuthorityDbContext<TDbContext>>(sp =>
        {
            var dbContext = sp.GetRequiredService<TDbContext>();
            return new AuthorityDbContext<TDbContext>(dbContext);
        });
        services.AddScoped<AuthorityService>();
    }
    
    
}
