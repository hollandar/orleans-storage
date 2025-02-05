using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Webefinity.ContentRoot.Abstractions;
using Webefinity.ContentRoot.Hosting.Client;

namespace Webefinity.ContentRoot.Hosting;

public class ContentRootOptionsBuilder
{
    public string ConfigurationSection { get; set; } = "ContentRoot";
}

public static class StartupExtensions
{
    public static void AddContentRoot(this IServiceCollection services, string? key = null)
    {

        if (key is null)
        {
            services.AddScoped<IContentRootService, ContentRootService>();
        }
        else
        {
            services.AddKeyedScoped<IContentRootService, ContentRootService>(key, (sp, k) =>
            {
                ArgumentNullException.ThrowIfNull(k, nameof(k));
                var library = sp.GetKeyedService<IContentRootLibrary>((string)k);
                if (library is not null)
                {
                    return new ContentRootService(library);
                }
                else
                {
                    throw new InvalidOperationException($"No content root with key {key} is registered.");
                }
            });
        }
    }

    public static void MapContentRootServer(this IEndpointRouteBuilder endpoints, string? key = null)
    {
        string? mapKey = key;
        var uri = "/content/{Collection}/{**Path}";
        if (mapKey is not null)
            uri = "/content/" + mapKey.ToLower() + "/{Collection}/{**Path}";

        endpoints.MapGet(uri, async (
            string Collection, 
            string Path, 
            IServiceProvider serviceProvider, 
            IHttpContextAccessor httpContextAccessor
        ) =>
        {
            IContentRootLibrary contentRootLibrary;
            if (mapKey is null)
            {
                contentRootLibrary = serviceProvider.GetService<IContentRootLibrary>() ?? throw new InvalidOperationException("No content root is registered.");
            }
            else
            {
                contentRootLibrary = serviceProvider.GetKeyedService<IContentRootLibrary>(mapKey) ?? throw new InvalidOperationException($"No content root with key {mapKey} is registered.");
            }

            var collectionDef = new CollectionDef(Collection);
            if (await contentRootLibrary.FileExistsAsync(collectionDef, Path))
            {
                return Results.Stream(await contentRootLibrary.LoadReadStreamAsync(collectionDef, Path), ContentTypes.GetContentType(Path));
            }
            else
            {
                return Results.NotFound();
            }
        });
    }
}
