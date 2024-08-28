using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Pages.Services;

namespace Webefinity.Module.Blog
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddWebefinityPages(this IServiceCollection services)
        {
            services.AddScoped<PageCacheService>();

            return services;
        }

        public static RazorComponentsEndpointConventionBuilder AddWebefinityPageComponents(this RazorComponentsEndpointConventionBuilder builder)
        {
            builder.AddAdditionalAssemblies(typeof(StartupExtensions).Assembly);

            return builder;
        }
    }
}
