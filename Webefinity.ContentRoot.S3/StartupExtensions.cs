using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.ContentRoot.S3;

public static class StartupExtensions
{
    public static void ConfigureContentRootS3(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.AddScoped<IContentRootLibrary, ContentRootS3>();
        }
        else
        {
            services.AddKeyedScoped<IContentRootLibrary, ContentRootS3>(key, (sp, k) =>
            {
                ArgumentNullException.ThrowIfNull(k, nameof(k));
                var options = sp.GetRequiredService<IOptions<ContentRootOptionsBase>>();
                if (options.Value.Options.TryGetValue((string)k, out var contentRootOptions)) {
                    return new ContentRootS3(sp, Options.Create(contentRootOptions));
                }
                else
                {
                    throw new InvalidOperationException($"ContentRootOptionsBase.Options does not contain key {key}");
                }
            });
        }
    }
}
