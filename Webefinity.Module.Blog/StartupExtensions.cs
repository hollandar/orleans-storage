using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Webefinity.ContentRoot;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Services;

namespace Webefinity.Module.Blog
{
    public static class StartupExtensions
    {

        public static void AddWebefinityBlog<TDbContext>(this WebApplicationBuilder builder, string? key = null) where TDbContext : DbContext
        {
            builder.Services.AddScoped<ArticleIndexService>();
            builder.Services.AddScoped<IBlogDbContext, BlogDbContext<TDbContext>>((sp) => new BlogDbContext<TDbContext>(sp.GetRequiredService<TDbContext>()));
            builder.Services.AddHostedService((sp) => new ArticleIndexHostedService(sp, key));
            builder.Services.AddKeyedSingleton<string>("Webefinity.Module.Blog.ArticleStoreKey", key!);
        }

        public static void MapWebefinityBlogEndpoints(this WebApplication builder, string? key = null)
        {
            
            builder.Map("/contentroot/blog/{*slug}", (string slug, IServiceProvider serviceProvider) =>
            {
                var contentRootLibrary = key is not null? serviceProvider.GetRequiredKeyedService<IContentRootLibrary>(key) : serviceProvider.GetRequiredService<IContentRootLibrary>();
                if (contentRootLibrary.FileExists(Constants.BlogCollection, slug))
                    return Results.File(contentRootLibrary.LoadReadStream(Constants.BlogCollection, slug));
                else
                    return Results.NotFound();
            });
        }

        public static RazorComponentsEndpointConventionBuilder AddWebefinityBlogComponents(this RazorComponentsEndpointConventionBuilder builder)
        {
            builder.AddAdditionalAssemblies(typeof(StartupExtensions).Assembly);

            return builder;
        }
    }
}
