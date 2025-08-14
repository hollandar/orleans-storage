using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System.Reflection;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Services;
using Webefinity.Module.SiteMap.Abstractions;

namespace Webefinity.Module.Blocks;

public static class SetupExtensions
{
    public static void AddBlocksProvider(this IServiceCollection services, BlockSecurityPolicies securityPolicies, params Assembly[] additionalBlockAssemblies)
    {
        BlockProviderService.AddAssemblies(typeof(SetupExtensions).Assembly);
        BlockProviderService.AddAssemblies(additionalBlockAssemblies);
        services.AddScoped<BlocksProviderService>();
        services.AddScoped<BlockProviderService>();
        services.AddSingleton<BlockSecurityPolicies>(securityPolicies);
        services.AddScoped<ISitemapGenerator, PageSitemapGeneratorService>();
    }

    public static void AddPageControlLink(this IServiceCollection services, string icon, string alt, string link, int order = 0, string? text = null)
    {
        services.AddSingleton(new PageControlLink(icon, alt, link, order, text));
    }   
}
