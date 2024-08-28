using Microsoft.AspNetCore.Builder;

namespace Webefinity.Module.Blog
{
    public static class StartupExtensions
    {
        public static RazorComponentsEndpointConventionBuilder AddWebefinityPageComponents(this RazorComponentsEndpointConventionBuilder builder)
        {
            builder.AddAdditionalAssemblies(typeof(StartupExtensions).Assembly);

            return builder;
        }
    }
}
