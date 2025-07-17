using System;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Abstractions;
using Webefinity.Module.Guides.Services;

namespace Webefinity.Module.Guides;

public static class StartupExtensions
{
    public static void AddGuideClientService(this IServiceCollection services)
    {
        services.AddScoped<IGuideService, GuideClientService>();
    }
}
