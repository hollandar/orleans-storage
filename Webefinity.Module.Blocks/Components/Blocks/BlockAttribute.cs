namespace Webefinity.Module.Blocks.Components.Blocks;

internal class BlockAttribute : Attribute
{
    public string Kind { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public BlockAttribute() : base()
    {

    }

    public BlockAttribute(string kind, string? name = null, string? description = null) : this()
    {
        this.Kind = kind;
        this.Name = name ?? kind;
        this.Description = description ?? string.Empty;
    }
}
