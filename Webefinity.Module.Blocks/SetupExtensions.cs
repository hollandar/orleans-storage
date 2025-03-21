using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Blocks.Services;

namespace Webefinity.Module.Blocks;

public static class SetupExtensions
{
    public static void AddBlocksProvider(this IServiceCollection services)
    {
        services.AddScoped<BlocksProviderService>();
        services.AddScoped<BlockProviderService>();
    }
}
