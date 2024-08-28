using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Webefinity.ContentRoot;
using Webefinity.Module.Blog.Data;
using Webefinity.Module.Blog.Services;

namespace Webefinity.Module.Blog
{
    public static class StartupExtensions
    {

        public static void AddWebefinityBlog(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ArticleIndexService>();
            var blogConnectionString = builder.Configuration.GetConnectionString("Blog");
            builder.Services.AddDbContextFactory<BlogDbContext>(options =>
            {
                options.UseSqlite(blogConnectionString ?? "Data Source=db/blog.db");
            });
            builder.Services.AddHostedService<ArticleIndexHostedService>();
        }

        public static void MapWebefinityBlogEndpoints(this WebApplication builder)
        {
            var contentRootOptions = builder.Services.GetRequiredService<IContentRootLibrary>();
            
            builder.Map("/contentroot/blog/{*slug}", (string slug, IContentRootLibrary contentRootLibrary) =>
            {

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
