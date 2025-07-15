using Webefinity.Module.Blocks.Abstractions;
using Webefinity.Module.Blocks.Data.Entities;

namespace Webefinity.Module.Blocks.Data.Mappers;

public static class PageMapper
{
    public static PageModel Map(Page page) => new PageModel
    {
        Id = page.Id,
        Name = page.Name,
        Title = page.Title,
        State = page.State,
        Blocks = page.Blocks.Select(BlockMapper.Map).ToList()
    };
}
