
using Webefinity.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Webefinity.ContentRoot;
using Webefinity.Module.Pages.Options;
using Microsoft.Extensions.Options;

namespace Webefinity.Module.Pages.Services;

public record PageCache(
    string Title,
    string Content);

public class PageFrontmatter
{
    public string Route { get; set; } = string.Empty;
    public string? Title { get; set; } = null;
}

public class PageCacheService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IContentRootLibrary contentRootLibrary;
    private readonly IOptions<PageRouterOptions> options;
    private IMemoryCache? memoryCache;


    public PageCacheService(IServiceProvider serviceProvider, IContentRootLibrary contentRootLibrary, IOptions<PageRouterOptions> options)
    {
        this.serviceProvider = serviceProvider;
        this.contentRootLibrary = contentRootLibrary;
        this.options = options;
        memoryCache = this.serviceProvider.GetService<IMemoryCache>();
    }

    public async Task<ValueResult<PageCache>> GetPageAsync(string? route)
    {
        var pageRoute = $"/{route}";

        if (this.memoryCache is not null && this.memoryCache.TryGetValue(pageRoute, out PageCache? pageCache))
        {
            return ValueResult<PageCache>.Ok(pageCache!);
        }

        var collection = new CollectionDef(options.Value.Collection);
        await foreach (var page in contentRootLibrary.EnumerateRecursiveAsync(collection, "*.*"))
        {
            using var pageReader = contentRootLibrary.LoadReader(collection, page);
            var frontmatterContent = await Frontmatter.FrontmatterLoader.LoadAsync<PageFrontmatter>(pageReader);

            if (frontmatterContent.Frontmatter?.Route == pageRoute)
            {
                var title = frontmatterContent.Frontmatter.Title ?? route ?? "Page";
                var content = string.Empty;
                var contentType = ContentTypes.GetContentType(page);
                switch (contentType)
                {
                    case "text/markdown":
                        content = Markdig.Markdown.ToHtml(frontmatterContent.Content);
                        break;
                    case "text/htm":
                    case "text/html":
                        content = frontmatterContent.Content;
                        break;
                    default:
                        throw new Exception($"Unsupported content type {contentType}.");

                }

                pageCache = new PageCache(title, content);

                if (this.memoryCache is not null)
                {
                    this.memoryCache.Set(pageRoute, pageCache, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    });
                }

                return ValueResult<PageCache>.Ok(pageCache);
            }
        }

        return ValueResult<PageCache>.Fail("Page not found.", ResultReasonType.NotFound);
    }
}
