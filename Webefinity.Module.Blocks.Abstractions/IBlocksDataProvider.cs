
using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Abstractions;

public interface IBlocksDataProvider
{
    Task<PageExistsModel> PageExistsAsync(string name, CancellationToken ct);
    Task<PageModel> GetPageModelAsync(string name, CancellationToken ct);
    Task<bool> SetPageModelAsync(BlockModel model, JsonDocument jsonDocument, CancellationToken ct);
    Task<bool> AddBlockAtAsync(Guid pageId, string kind, int sequence, CancellationToken ct);
    Task<bool> DeleteBlockAsync(Guid blockId, CancellationToken ct);
    Task<bool> CreatePageAsync(CreatePageModel createPageModel, CancellationToken ct);
    Task<bool> DeletePageAsync(Guid pageId, CancellationToken ct);
    Task<bool> MoveBlockAsync(Guid blockId, MoveDirection moveDirection, CancellationToken ct);
    Task UpdatePageAsync(UpdateBlockSettingsRequest settingsModel, CancellationToken ct);
    Task<PublishState> PublishPageAsync(Guid pageId, PublishState publishState, CancellationToken ct);
}
