namespace Webefinity.Module.Blocks.Abstractions;

public class PageModel
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = "No Title Provided";
    public PublishState State { get; set; } = PublishState.Draft;
    public ICollection<BlockModel> Blocks { get; set; } = Array.Empty<BlockModel>();
}
