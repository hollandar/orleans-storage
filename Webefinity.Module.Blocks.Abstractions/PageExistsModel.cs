using System;

namespace Webefinity.Module.Blocks.Abstractions;

public class PageExistsModel
{
    public bool Exists { get; set; } = false;
    public Guid? PageId { get; set; } = null;
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PublishState State { get; set; } = PublishState.Draft;

    public PageExistsModel() { }

    public PageExistsModel(bool exists, Guid? pageId, string title, string name, PublishState state = PublishState.Draft)
    {
        Exists = exists;
        PageId = pageId;
        Title = title;
        Name = name;
        State = state;
    }

    public static PageExistsModel DoesNotExist { get => new PageExistsModel(false, null, string.Empty, string.Empty); }
}
