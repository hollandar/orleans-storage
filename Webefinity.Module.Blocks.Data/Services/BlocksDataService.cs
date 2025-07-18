﻿using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;
using Webefinity.Module.Blocks.Data.Mappers;

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

        var maxSequence = blocks.Max(r => (int?)r.Sequence) ?? 0;
        if (sequence > maxSequence)
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
        var lowerName = name.ToLower() ?? string.Empty;
        var page = dbContextChild.Pages.Where(r => r.Name.ToLower() == lowerName).FirstOrDefault();
        if (page is null) throw new ArgumentException($"Page {name} not found", nameof(name));

        dbContextChild.Pages.Entry(page).Collection(r => r.Blocks).Load();
        return Task.FromResult(PageMapper.Map(page));
    }

    public Task<PageOutlineModel> GetPageOutlineAsync(string name, CancellationToken ct)
    {
        var lowerName = name.ToLower();
        var pages = dbContextChild.Pages.Where(r => r.Name.ToLower() == name.ToLower()).Select(r => new { r.Id, r.Title, r.Name, r.State });
        var pageExists = pages.Any();
        if (!pageExists)
        {
            return Task.FromResult(PageOutlineModel.DoesNotExist);
        }
        var page = pages.Single();

        return Task.FromResult(new PageOutlineModel(pageExists, page.Id, page.Title, page.Name, page.State));
    }

    public async Task<bool> SetPageModelAsync(BlockModel model, JsonDocument jsonDocument, CancellationToken ct)
    {
        var block = dbContextChild.Blocks.Where(r => r.Id == model.Id).SingleOrDefault();
        if (block is null)
        {
            return false;
        }

        block.Data = jsonDocument;

        MarkBlockPageAsModified(block.PageId);

        await dbContextChild.SaveChangesAsync(ct);

        return true;
    }

    private void MarkBlockPageAsModified(Guid pageId)
    {
        var page = dbContextChild.Pages.Find(pageId);
        if (page is null) return;

        page.UpdatedAt = DateTimeOffset.UtcNow;
        dbContextChild.Pages.Update(page);
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

        MarkBlockPageAsModified(block.PageId);

        await this.dbContextChild.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreatePageAsync(CreatePageModel createPageModel, CancellationToken ct)
    {
        var validator = new CreatePageModelValidator();
        var validationResult = validator.Validate(createPageModel);
        if (!validationResult.IsValid)
        {
            return false;
        }

        var page = new Page { Id = Guid.CreateVersion7(), Name = createPageModel.PageName, Title = createPageModel.PageTitle, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, State = PublishState.Draft };
        this.dbContextChild.Pages.Add(page);

        await this.dbContextChild.SaveChangesAsync();

        return true;

    }

    public async Task<bool> DeletePageAsync(Guid pageId, CancellationToken ct)
    {
        await this.dbContextChild.Blocks.Where(r => r.PageId == pageId).ExecuteDeleteAsync();
        await this.dbContextChild.Pages.Where(r => r.Id == pageId).ExecuteDeleteAsync();

        return true;
    }

    public async Task<bool> MoveBlockAsync(Guid blockId, MoveDirection moveDirection, CancellationToken ct)
    {

        var blockData = this.dbContextChild.Blocks.Where(r => r.Id == blockId).Select(r => new { PageId = r.PageId, BlockSequence = r.Sequence }).SingleOrDefault();
        if (blockData is null)
        {
            return false;
        }

        var blocks = this.dbContextChild.Blocks.Where(r => r.PageId == blockData.PageId && r.Sequence >= blockData.BlockSequence - 1 && r.Sequence <= blockData.BlockSequence + 1).OrderBy(r => r.Sequence).ToArray();

        if (blocks.Length <= 1)
        {
            return false;
        }

        Block movingDown;
        Block movingUp;
        switch (moveDirection)
        {
            case MoveDirection.Up:
                if (blocks[0].Id == blockId) return false;
                movingDown = blocks[0];
                movingUp = blocks[1];
                break;
            case MoveDirection.Down:
                if (blocks[^1].Id == blockId) return false;
                movingDown = blocks[^2];
                movingUp = blocks[^1];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(moveDirection), moveDirection, null);
        }

        var movingUpSequence = movingUp.Sequence;
        var movingDownSequence = movingDown.Sequence;

        do
        {
            movingUp.Sequence = -Random.Shared.Next();
        } while (this.dbContextChild.Blocks.Where(r => r.Sequence == movingUp.Sequence).Any());

        await this.dbContextChild.SaveChangesAsync();
        do
        {
            movingDown.Sequence = -Random.Shared.Next();
        } while (this.dbContextChild.Blocks.Where(r => r.Sequence == movingDown.Sequence).Any());

        await this.dbContextChild.SaveChangesAsync(ct);

        movingUp.Sequence = movingDownSequence;
        movingDown.Sequence = movingUpSequence;

        MarkBlockPageAsModified(blockData.PageId);

        await this.dbContextChild.SaveChangesAsync(ct);

        return true;
    }

    public async Task UpdatePageAsync(UpdateBlockSettingsRequest settingsModel, CancellationToken ct)
    {
        var page = this.dbContextChild.Pages.Find(settingsModel.Id);
        if (page is null)
        {
            throw new ArgumentException($"Page with ID {settingsModel.Id} not found", nameof(settingsModel.Id));
        }

        page.Name = settingsModel.Name;
        page.Title = settingsModel.Title;
        page.State = settingsModel.State;
        page.UpdatedAt = DateTimeOffset.UtcNow;
        await this.dbContextChild.SaveChangesAsync(ct);
    }

    public async Task<PublishState> PublishPageAsync(Guid pageId, PublishState publishState, CancellationToken ct)
    {
        var page = this.dbContextChild.Pages.Find(pageId);
        if (page is null)
        {
            throw new ArgumentException($"Page with ID {pageId} not found", nameof(pageId));
        }

        if (!page.State.GetAllowedTransitions().Contains(publishState))
        {
            throw new InvalidOperationException($"Cannot transition from {page.State.ToDisplayString()} to {publishState.ToDisplayString()}");
        }

        page.State = publishState;
        page.UpdatedAt = DateTimeOffset.UtcNow;
        await this.dbContextChild.SaveChangesAsync(ct);

        return page.State;
    }
}
