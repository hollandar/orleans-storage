using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.Content.FileIndex.Tests;

public class IndexStoreTests
{
    IServiceProvider serviceProvider;
    public IndexStoreTests()
    {
        var contentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
        if (Directory.Exists(contentRootPath))
        {
            Directory.Delete(contentRootPath, true);
        }
        Directory.CreateDirectory(contentRootPath);

        var services = new ServiceCollection();
        services.AddOptions<ContentRootOptions>().Configure(o => o.Properties["Path"] = contentRootPath);
        services.ConfigureContentRootFile();

        this.serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task WriteFile()
    {
        var lib = this.serviceProvider.GetRequiredService<IContentRootLibrary>();
        await lib.SaveAsync(CollectionDef.DefaultCollection, "file.json", new MemoryStream(Encoding.UTF8.GetBytes("{\"test\": \"test\"}")), "application/json");        
    }
}
