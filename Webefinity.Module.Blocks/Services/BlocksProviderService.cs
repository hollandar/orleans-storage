using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Services;

public class BlocksProviderService
{
    private readonly IBlocksDataProvider blocksDataProvider;

    public BlocksProviderService(IBlocksDataProvider blocksDataProvider)
    {
        this.blocksDataProvider = blocksDataProvider;
    }

    public Task<PageExistsModel> PageExistsAsync(string name, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.PageExistsAsync(name, ct);
    }

    public Task<PageModel> GetPageModelAsync(string name, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.GetPageModelAsync(name, ct);
    }

    public Task<bool> SetBlockModelAsync(BlockModel model, JsonDocument jsonDocument, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.SetPageModelAsync(model, jsonDocument, ct);
    }

    public Task<bool> AddBlockAtAsync(Guid pageId, string kind, int sequence, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.AddBlockAtAsync(pageId, kind, sequence, ct);
    }

    public Task<bool> DeleteBlockAsync(Guid blockId, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.DeleteBlockAsync(blockId, ct);
    }

    public Task<bool> DeletePageAsync(Guid pageId, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.DeletePageAsync(pageId, ct);
    }

    public Task<bool> CreatePageAsync(CreatePageModel createPageModel, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.CreatePageAsync(createPageModel, ct);
    }

    public Task<bool> MoveBlockAsync(Guid blockId, MoveDirection moveDirection, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.MoveBlockAsync(blockId, moveDirection, ct);
    }
    
    public Task UpdatePageAsync(UpdateBlockSettingsRequest settingsModel, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.UpdatePageAsync(settingsModel, ct);
    }
}
