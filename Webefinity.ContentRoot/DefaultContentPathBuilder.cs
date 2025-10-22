using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.ContentRoot;

public class DefaultContentPathBuilder : IContentPathBuilder
{
    protected bool NotNull(string? item) => item is not null && !String.IsNullOrWhiteSpace(item);

    protected void AddBasePath(List<string> items, string? basePath)
    {
        if (basePath is null) return;
        items.AddRange(basePath.Split('/'));
    }

    protected void AddFolder(List<string> items, string? folder)
    {
        if (folder is null) return;
        items.AddRange(folder.Split('/'));
    }

    protected void AddCollection(List<string> items, CollectionDef collection)
    {
        items.Add(collection.Collection);
    }

    public virtual string GetPath(CollectionDef collection, string? folder = null, string? basePath = null)
    {
        var pathItems = new List<string>(6);
        AddBasePath(pathItems, basePath);
        if (collection != CollectionDef.RootCollection)
        {
            AddCollection(pathItems, collection);
        }

        AddFolder(pathItems, folder);

        return Combine(pathItems);
    }

    protected string Combine(List<string> items) => String.Join("/", items.Where(NotNull));
}
