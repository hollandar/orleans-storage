namespace Webefinity.Module.Blog.Models;

public class ArticleFrontmatter
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.MinValue;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? Image { get; set; } = null;
    public string? Summary { get; set; } = null;
}
