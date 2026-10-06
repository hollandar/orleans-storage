namespace Webefinity.Module.Blog.Options;

public class BlogOptions
{
    public string? SiteTitle { get; set; } = null;
    public string[] AuthorSecurityPolicies { get; set; } = [];
    public string ArticleIndexMetaDescription { get; set; } = "An index of articles.";
    public string ArticleIndexTitle { get; set; } = "Articles.";
}
