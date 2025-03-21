
using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Modules.Blocks.Abstractions;

public interface IBlocksDataProvider
{
    Task<bool> PageExistsAsync(string name, CancellationToken ct);
    Task<PageModel> GetPageModelAsync(string name, CancellationToken ct);
    Task<bool> SetPageModelAsync(BlockModel model, JsonDocument jsonDocument, CancellationToken ct);
    Task<bool> AddBlockAtAsync(Guid pageId, string kind, int sequence, CancellationToken ct);
    Task<bool> DeleteBlockAsync(Guid blockId, CancellationToken ct);
}
