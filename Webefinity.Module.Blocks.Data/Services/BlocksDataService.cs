using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;
using Webefinity.Module.Blocks.Data.Mappers;
using Webefinity.Modules.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Data.Services;

public class BlocksDataService : IBlocksDataProvider
{
    private readonly IBlocksDbContextChild dbContextChild;

    public BlocksDataService(IBlocksDbContextChild dbContextChild)
    {
        this.dbContextChild = dbContextChild;
    }

    public Task<PageModel> GetPageModelAsync(string name, CancellationToken ct)
    {
        var page = dbContextChild.Pages.Where(r => r.Name.ToLower() == name.ToLower()).FirstOrDefault();
        if (page is null) throw new ArgumentException($"Page {name} not found", nameof(name));

        dbContextChild.Pages.Entry(page).Collection(r => r.Blocks).Load();
        return Task.FromResult(PageMapper.Map(page));
    }

    public Task<bool> PageExistsAsync(string name, CancellationToken ct)
    {
        var lowerName = name.ToLower();
        var pageExists = dbContextChild.Pages.Where(r => r.Name.ToLower() == name.ToLower()).Any();
        return Task.FromResult(pageExists);
    }

    public async Task<bool> SetPageModelAsync(BlockModel model, JsonDocument jsonDocument, CancellationToken ct)
    {
        var block = dbContextChild.Blocks.Where(r => r.Id == model.Id).SingleOrDefault();
        if (block is null)
        {
            return false;
        }

        block.Data = jsonDocument;
        await dbContextChild.SaveChangesAsync(ct);

        return true;
    }
}
