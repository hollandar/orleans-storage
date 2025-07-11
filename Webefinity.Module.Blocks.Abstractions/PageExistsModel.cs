using System;

namespace Webefinity.Module.Blocks.Abstractions;

public class PageExistsModel
{
    public bool Exists { get; set; } = false;
    public Guid? PageId { get; set; } = null;
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public PageExistsModel() { }

    public PageExistsModel(bool exists, Guid? pageId, string title, string name)
    {
        Exists = exists;
        PageId = pageId;
        Title = title;
        Name = name;
    }

    public static PageExistsModel DoesNotExist { get => new PageExistsModel(false, null, string.Empty, string.Empty); }
}
