using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;

namespace Webefinity.Module.Blocks.Services;

public class BlocksProviderService
{
    private readonly IBlocksDataProvider blocksDataProvider;
    private readonly BlockSecurityPolicies blockSecurityPolicies;
    private readonly IAuthorizationService authorizationService;
    private readonly AuthenticationStateProvider authenticationStateProvider;

    public BlocksProviderService(IBlocksDataProvider blocksDataProvider, BlockSecurityPolicies blockSecurityPolicies, IAuthorizationService authorizationService, AuthenticationStateProvider authenticationStateProvider)
    {
        this.blocksDataProvider = blocksDataProvider;
        this.blockSecurityPolicies = blockSecurityPolicies;
        this.authorizationService = authorizationService;
        this.authenticationStateProvider = authenticationStateProvider;
    }

    public Task<PageOutlineModel> GetPageOutlineAsync(string name, CancellationToken ct = default!)
    {
        return this.blocksDataProvider.GetPageOutlineAsync(name, ct);
    }

    public Task<PageResult> GetPageModelAsync(string name, CancellationToken ct = default!)
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

    public Task<PublishState> PublishPageAsync(Guid pageId, PublishState publishState, CancellationToken ct)
    {
        return this.blocksDataProvider.PublishPageAsync(pageId, publishState, ct);
    }

    public async Task<IEnumerable<PageListModel>> ListPagesAsync(CancellationToken ct = default!)
    {
        if (this.blockSecurityPolicies.AuthorPolicy is null)
        {
            throw new InvalidOperationException("AuthorPolicy is not configured.");
        }
        var user = await this.authenticationStateProvider.GetAuthenticationStateAsync();
        var authorizeResult = await this.authorizationService.AuthorizeAsync(user.User, this.blockSecurityPolicies.AuthorPolicy);
        return await this.blocksDataProvider.GetPageListAsync(ct);
    }
}
