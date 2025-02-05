using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.ContentRoot.Hosting.Client;

public static class StartupExtensions
{
    public static void AddContentRootClient(this IServiceCollection services, string? key = null)
    {
        if (key is null)
        {
            services.AddScoped<IContentRootService>((sp) => new ContentRootClientService(sp.GetRequiredService<HttpClient>(), null));
        } 
        
        else
        {
            services.AddKeyedScoped<IContentRootService>(key, (sp, k) => {
                ArgumentNullException.ThrowIfNull(k, nameof(k));
                return new ContentRootClientService(sp.GetRequiredService<HttpClient>(), (string)k);
            });
        }
    }
}
