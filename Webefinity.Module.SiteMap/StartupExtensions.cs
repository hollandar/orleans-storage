using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Webefinity.Module.SiteMap;

public static class StartupExtensions
{
    public static IServiceCollection AddSitemapGenerator(this IServiceCollection services)
    {
        services.AddScoped<SitemapGeneratorService>();

        return services;
    }

    public static void MapSitemapGenerator(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/sitemap.xml", (HttpContext context) =>
        {
            var sitemapService = context.RequestServices.GetRequiredService<SitemapGeneratorService>();
            var pathBase = context.Request.PathBase.ToString().Trim('/');
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/{pathBase}";
            var xmlDoc = sitemapService.GenerateRootSitemap(baseUrl);

            return Results.Content(xmlDoc.OuterXml, "application/xml");
        });
        routeBuilder.MapGet("/{section}/sitemap.xml", async (HttpContext context, string section) =>
        {
            var sitemapService = context.RequestServices.GetRequiredService<SitemapGeneratorService>();
            var pathBase = context.Request.PathBase.ToString().Trim('/');
            var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/{pathBase}";
            var xmlDoc = await sitemapService.GenerateSectionSitemap(baseUrl, section);

            return Results.Content(xmlDoc.OuterXml, "application/xml");
        });
    }
}
