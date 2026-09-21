using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Webefinity.ContentRoot;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Services;
using Webefinity.Module.Scheduler;
using Webefinity.Module.Scheduler.Conditions;

namespace Webefinity.Module.Blog;

public static class StartupExtensions
{

    public static void AddWebefinityBlog<TDbContext>(this WebApplicationBuilder builder) where TDbContext : DbContext
    {
        builder.Services.AddScoped<ArticleIndexService>();
        builder.Services.AddScoped<ArticleContentService>();
        builder.Services.AddScoped<IBlogDbContext, BlogDbContext<TDbContext>>((sp) => new BlogDbContext<TDbContext>(sp.GetRequiredService<TDbContext>()));
        builder.Services.AddSchedulerJobs(options =>
        {
            options.ScheduleJob<ArticleIndexJob>("ArticleIndexer", new ConditionAtStartup(), new ConditionManual(Constants.BlobReindexTrigger)); 
        });
        builder.Services.AddHttpContextAccessor();
    }

    public static void MapWebefinityBlogEndpoints(this WebApplication builder)
    {
        builder.Map("/contentroot/blog/{*slug}", (string slug, IServiceProvider serviceProvider) =>
        {
            return Results.NotFound();
        });
    }

    public static RazorComponentsEndpointConventionBuilder AddWebefinityBlogComponents(this RazorComponentsEndpointConventionBuilder builder)
    {
        builder.AddAdditionalAssemblies(typeof(StartupExtensions).Assembly);

        return builder;
    }
}
