using System;
using System.Net.WebSockets;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;
using Webefinity.Module.SiteMap.Abstractions;
namespace Webefinity.Module.Blocks.Services;

public class PageSitemapGeneratorService : ISitemapGenerator
{
    private readonly IBlocksDbContextChild dbContext;

    public PageSitemapGeneratorService(IBlocksDbContextChild dbContext)
    {
        this.dbContext = dbContext;
    }
    public SitemapSection GetSection()
    {
        var lastModified = this.dbContext.Pages
            .Where(p => p.State == PublishState.Published)
            .Select(p => p.UpdatedAt)
            .ToList()
            .DefaultIfEmpty(DateTimeOffset.MinValue)
            .Max();

        return new SitemapSection("pages", lastModified);
    }

    public Task<IEnumerable<SitemapNode>> GetSectionContentAsync()
    {
        var pages = this.dbContext.Pages
            .Where(p => p.State == PublishState.Published)
            .Select(p => new { p.Name, p.UpdatedAt })
            .ToList()
            .Select(p =>
            {
                var pageName = p.Name.ToLowerInvariant();
                if (pageName == "home")
                {
                    pageName = "";
                }

                return new SitemapNode(pageName, p.UpdatedAt, "weekly", 0.5f);
            });

        return Task.FromResult(pages);
    }
}
