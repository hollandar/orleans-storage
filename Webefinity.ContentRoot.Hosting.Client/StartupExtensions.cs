using Microsoft.Extensions.DependencyInjection;

namespace Webefinity.ContentRoot.Hosting.Client;

public static class StartupExtensions
{
    public static void AddContentRootClient(this IServiceCollection services)
    {
        services.AddScoped<IContentRootService, ContentRootClientService>();
    }
}
