namespace Webefinity.Module.Blog.Data;

public class BlogArticle
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTimeOffset Date { get; set; } = DateTimeOffset.MinValue;
    public string? Image { get; set; } = null;
    public string? Summary { get; set; } = null;
    public virtual ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
    public virtual ICollection<BlogWord> Words { get; set; } = new List<BlogWord>();
}
