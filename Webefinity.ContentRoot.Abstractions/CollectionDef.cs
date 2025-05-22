namespace Webefinity.ContentRoot.Abstractions;

public class CollectionDef(string collection)
{
    public static readonly CollectionDef DefaultCollection = new CollectionDef("Default");

    public string Collection { get; } = collection;
}
