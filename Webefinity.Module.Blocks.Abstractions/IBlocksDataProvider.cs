
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Modules.Blocks.Abstractions;

public interface IBlocksDataProvider
{
    Task<bool> PageExistsAsync(string name, CancellationToken ct);
    Task<PageModel> GetPageModelAsync(string name, CancellationToken ct);
}
