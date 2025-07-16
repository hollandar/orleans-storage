using Webefinity.Module.SiteMap.Abstractions;

namespace Webefinity.Module.SiteMap.Abstractions;



public interface ISitemapGenerator
{
    SitemapSection GetSection();
    Task<IEnumerable<SitemapNode>> GetSectionContentAsync();
}
