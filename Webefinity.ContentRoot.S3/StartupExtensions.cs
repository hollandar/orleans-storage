using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Runtime.Intrinsics.Arm;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.ContentRoot.S3;

public static class StartupExtensions
{
    public static void ConfigureContentRootS3(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.TryAddScoped<IContentPathBuilder, DefaultContentPathBuilder>();
            services.AddScoped<IContentRootLibrary, ContentRootS3>();
        }
        else
        {
            services.AddKeyedScoped<IContentRootLibrary, ContentRootS3>(key, (sp, k) =>
            {
                ArgumentNullException.ThrowIfNull(k, nameof(k));
                var pathBuilder = sp.GetRequiredKeyedService<IContentPathBuilder>(k);
                var options = sp.GetRequiredService<IOptions<ContentRootOptionsBase>>();
                if (options.Value.Options.TryGetValue((string)k, out var contentRootOptions)) {
                    return new ContentRootS3(sp, Options.Create(contentRootOptions), pathBuilder);
                }
                else
                {
                    throw new InvalidOperationException($"ContentRootOptionsBase.Options does not contain key {key}");
                }
            });
            services.TryAddKeyedScoped<IContentPathBuilder, DefaultContentPathBuilder>(key);
        }
    }
}
