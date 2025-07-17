using System;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.Module.Guides.Server.Services;
using Webefinity.Module.Guides.Services;

namespace Webefinity.Module.Guides.Server;

public static class StartupExtensions
{
    public static void AddGuideServerService(this IServiceCollection services)
    {
        services.AddScoped<IGuideService, GuideServerService>();
    }
}