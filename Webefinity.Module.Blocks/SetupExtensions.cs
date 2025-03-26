using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Webefinity.Module.Blocks.Services;

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
    }
}
