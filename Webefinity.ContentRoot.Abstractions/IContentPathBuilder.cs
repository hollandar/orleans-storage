namespace Webefinity.ContentRoot.Abstractions;

public interface IContentPathBuilder
{
    string GetPath(CollectionDef collection, string? folder = null, string? basePath = null);
}
