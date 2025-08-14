namespace Webefinity.ContentRoot.IndexUI;

public record KeyCollection(string Key, string Collection)
{
    public Guid Id { get; } = Guid.NewGuid();
}
