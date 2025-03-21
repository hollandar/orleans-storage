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

    public async Task<bool> AddBlockAtAsync(Guid pageId, string kind, int sequence, CancellationToken ct)
    {
        var blocks = this.dbContextChild.Blocks.Where(r => r.PageId == pageId).ToList();

        var maxSequence = blocks.Max(r => r.Sequence);
        if ( sequence > maxSequence)
        {
            sequence = maxSequence + 1;
        }

        var newBlock = new Block
        {
            Id = Guid.CreateVersion7(),
            Kind = kind,
            PageId = pageId,
            Sequence = sequence,
            Data = JsonDocument.Parse("{}")
        };

        foreach (var block in blocks)
        {
            if (block.Sequence >= sequence)
            {
                block.Sequence++;
            }
        }

        dbContextChild.Blocks.Add(newBlock);
        await dbContextChild.SaveChangesAsync(ct);
        return true;
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

    public async Task<bool> DeleteBlockAsync(Guid blockId, CancellationToken ct)
    {
        var block = this.dbContextChild.Blocks.Find(blockId);
        if (block is null) return false;

        var deletedSequence = block.Sequence;
        var pageBlocks = this.dbContextChild.Blocks.Where(r => r.PageId == block.PageId && r.Id != blockId).OrderBy(r => r.Sequence).ToList();
        int newSequence = 0;
        foreach (var pageBlock in pageBlocks)
        {
            pageBlock.Sequence = newSequence++;
        }

        this.dbContextChild.Blocks.Remove(block);
        await this.dbContextChild.SaveChangesAsync();

        return true;
    }
}
