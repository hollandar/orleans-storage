namespace Webefinity.ContentRoot.Abstractions;

public record CollectionDef(string collection)
{
    public static readonly CollectionDef DefaultCollection = new CollectionDef("Default");
    public static readonly CollectionDef RootCollection = new CollectionDef(string.Empty);

    public string Collection { get; } = collection;
}
