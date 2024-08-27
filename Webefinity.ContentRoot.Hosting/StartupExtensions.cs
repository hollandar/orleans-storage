using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.ContentRoot.Hosting.Client;

namespace Webefinity.ContentRoot.Hosting;

public class ContentRootOptionsBuilder
{
    public string ConfigurationSection { get; set; } = "ContentRoot";
}

public static class StartupExtensions
{
    public static void AddContentRoot(this WebApplicationBuilder builder, Action<ContentRootOptionsBuilder>? action = null)
    {
        var crb = new ContentRootOptionsBuilder();
        if (action is not null)
        action(crb);

        var options = new ContentRootOptions();
        builder.Configuration.GetSection(crb.ConfigurationSection).Bind(options);

        switch (options.Type)
        {
            case ContentRootType.File:
                builder.Services.AddSingleton<IContentRootLibrary, ContentRootFile>();
                break;
            default:
                throw new Exception("Content root type not supported.");
        }

        builder.Services.AddScoped<IContentRootService, ContentRootService>();
    }

    public static void MapContentRootServer(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/content/{Collection}/{**Path}", (string Collection, string Path, IContentRootLibrary contentRootLibrary) => {
            var collectionDef = new CollectionDef(Collection);
            if (contentRootLibrary.FileExists(collectionDef, Path))
            {
                return Results.Stream(contentRootLibrary.LoadReadStream(collectionDef, Path), ContentTypes.GetContentType(Path));
            } else
            {
                return Results.NotFound("Content not found.");
            }
        });
    }
}
