namespace Webefinity.Module.Blocks.Components.Blocks;

public class BlockEditorAttribute : Attribute
{
    public string Kind { get; set; } = string.Empty;

    public BlockEditorAttribute() : base()
    {

    }

    public BlockEditorAttribute(string kind) : this()
    {
        this.Kind = kind;
    }
}
