using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Webefinity.ContentRoot.Abstractions;

public static class StartupExtensions
{
    public static void ConfigureContentRootFile(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.AddScoped<IContentRootLibrary, ContentRootFile>();
        }
        else
        {
            services.AddKeyedScoped<IContentRootLibrary, ContentRootFile>(key, (sp, k) =>
            {
                ArgumentNullException.ThrowIfNull(k, nameof(k));
                var options = sp.GetRequiredService<IOptions<ContentRootOptionsBase>>();
                if (options.Value.Options.TryGetValue((string)k, out var contentRootOptions)) {
                    return new ContentRootFile(sp, Options.Create(contentRootOptions));
                }
                else
                {
                    throw new InvalidOperationException($"ContentRootOptionsBase.Options does not contain key {key}");
                }
            });
        }
    }
}
